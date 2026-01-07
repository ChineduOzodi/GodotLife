using Godot;
using System.Collections.Generic;
using System;

namespace Goap
{
    public partial class GoapAgent
    {

        private readonly FSM stateMachine;

        private FSM.FSMState idleState; // finds something to do
        private FSM.FSMState moveToState; // moves to a target
        private FSM.FSMState performActionState; // performs an action

        private readonly HashSet<GoapAction> availableActions;
        private Queue<GoapAction> currentActions;

        private IGoap dataProvider; // this is the implementing class that provides our world data and listens to feedback on planning

        private readonly GoapPlanner planner;

        public Node2D entity;

        public GoapAgent() { }

        public GoapAgent(IGoap _dataProvider, GoapAction[] _actions)
        {
            stateMachine = new FSM();
            availableActions = [];
            currentActions = new Queue<GoapAction>();
            planner = new GoapPlanner();
            FindDataProvider(_dataProvider);
            entity = (Node2D)_dataProvider;
            CreateIdleState();
            CreateMoveToState();
            CreatePerformActionState();
            stateMachine.PushState(idleState);
            LoadActions(_actions);
        }


        public void Update(Node2D entity)
        {
            stateMachine.Update(entity);
        }


        public void AddAction(GoapAction a)
        {
            availableActions.Add(a);
        }

        public GoapAction GetAction(Type action)
        {
            foreach (GoapAction g in availableActions)
            {
                if (g.GetType().Equals(action))
                    return g;
            }
            return null;
        }

        public void RemoveAction(GoapAction action)
        {
            availableActions.Remove(action);
        }

        private bool HasActionPlan()
        {
            return currentActions.Count > 0;
        }

        private void CreateIdleState()
        {
            idleState = (fsm, gameObj) =>
            {
                // GOAP planning

                // get the world state and the goal we want to plan for
                HashSet<KeyValuePair<string, object>> worldState = dataProvider.GetWorldState();
                HashSet<KeyValuePair<string, object>> goal = dataProvider.CreateGoalState();

                // Plan
                Queue<GoapAction> plan = planner.Plan(entity, availableActions, worldState, goal);
                if (plan != null)
                {
                    // we have a plan, hooray!
                    currentActions = plan;
                    dataProvider.PlanFound(goal, plan);
                    //Debug.Log("Found plan");
                    fsm.PopState(); // move to PerformAction state
                    fsm.PushState(performActionState);

                }
                else
                {
                    // ugh, we couldn't get a plan
                    //        if (GameManager.Instance.debugLog)
                    //Debug.Log("<color=orange>Failed Plan:</color>"+prettyPrint(goal));
                    dataProvider.PlanFailed(goal);
                    fsm.PopState(); // move back to IdleAction state
                    fsm.PushState(idleState);
                }

            };
        }

        private void CreateMoveToState()
        {
            moveToState = (fsm, gameObj) =>
            {
                // move the game object

                GoapAction action = currentActions.Peek();
                if (action.RequiresInRange() && action.target == null)
                {
                    //Debug.Log("<color=red>Fatal error:</color> Action requires a target but has none. Planning failed. You did not assign the target in your Action.checkProceduralPrecondition()");
                    fsm.PopState(); // move
                    fsm.PopState(); // perform
                    fsm.PushState(idleState);
                    return;
                }

                // get the agent to move itself
                if (dataProvider.MoveAgent(action))
                {
                    fsm.PopState();
                }

                /*MovableComponent movable = (MovableComponent) gameObj.GetComponent(typeof(MovableComponent));
                if (movable == null) {
                    Debug.Log("<color=red>Fatal error:</color> Trying to move an Agent that doesn't have a MovableComponent. Please give it one.");
                    fsm.popState(); // move
                    fsm.popState(); // perform
                    fsm.pushState(idleState);
                    return;
                }
                float step = movable.moveSpeed * Time.deltaTime;
                gameObj.transform.position = Vector3.MoveTowards(gameObj.transform.position, action.target.transform.position, step);
                if (gameObj.transform.position.Equals(action.target.transform.position) ) {
                    // we are at the target location, we are done
                    action.setInRange(true);
                    fsm.popState();
                }*/
            };
        }

        private void CreatePerformActionState()
        {

            performActionState = (fsm, gameObj) =>
            {
                // perform the action

                if (!HasActionPlan())
                {
                    // no actions to perform
                    //Debug.Log("<color=red>Done actions</color>");
                    fsm.PopState();
                    fsm.PushState(idleState);
                    dataProvider.ActionsFinished();
                    return;
                }

                GoapAction action = currentActions.Peek();
                if (action.IsDone())
                {
                    // the action is done. Remove it so we can perform the next one
                    currentActions.Dequeue();
                }

                if (HasActionPlan())
                {
                    // perform the next action
                    action = currentActions.Peek();
                    bool inRange = !action.RequiresInRange() || action.IsInRange();

                    if (inRange)
                    {
                        // we are in range, so perform the action
                        bool success = action.Perform(gameObj);

                        if (!success)
                        {
                            // action failed, we need to plan again
                            fsm.PopState();
                            fsm.PushState(idleState);
                            dataProvider.PlanAborted(action);
                        }
                    }
                    else
                    {
                        // we need to move there first
                        // push moveTo state
                        fsm.PushState(moveToState);
                    }

                }
                else
                {
                    // no actions left, move to Plan state
                    fsm.PopState();
                    fsm.PushState(idleState);
                    dataProvider.ActionsFinished();
                }

            };
        }

        private void FindDataProvider(IGoap provider)
        {
            dataProvider = provider;
        }

        private void LoadActions(GoapAction[] _actions)
        {
            GoapAction[] actions = _actions;
            foreach (GoapAction a in actions)
            {
                availableActions.Add(a);
            }
            //Debug.Log("Found actions: "+prettyPrint(actions));
        }

        public static string PrettyPrint(HashSet<KeyValuePair<string, object>> state)
        {
            string s = "";
            foreach (KeyValuePair<string, object> kvp in state)
            {
                s += kvp.Key + ":" + kvp.Value.ToString();
                s += ", ";
            }
            return s;
        }

        public static string PrettyPrint(Queue<GoapAction> actions)
        {
            string s = "";
            foreach (GoapAction a in actions)
            {
                s += a.GetType().Name;
                s += "-> ";
            }
            s += "GOAL";
            return s;
        }

        public static string PrettyPrint(GoapAction[] actions)
        {
            string s = "";
            foreach (GoapAction a in actions)
            {
                s += a.GetType().Name;
                s += ", ";
            }
            return s;
        }

        public static string PrettyPrint(GoapAction action)
        {
            string s = "" + action.GetType().Name;
            return s;
        }
    }
}