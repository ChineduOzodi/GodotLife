using Godot;
using System;
using System.Collections.Generic;

namespace Goap
{

    /**
     * Plans what actions can be completed in order to fulfill a goal state.
     */
    public partial class GoapPlanner
    {

        /**
         * Plan what sequence of actions can fulfill the goal.
         * Returns null if a plan could not be found, or a list of the actions
         * that must be performed, in order, to fulfill the goal.
         */
        public Queue<GoapAction> Plan(Node2D agent,
                                      HashSet<GoapAction> availableActions,
                                      HashSet<KeyValuePair<string, object>> worldState,
                                      HashSet<KeyValuePair<string, object>> goal)
        {
            // reset the actions so we can start fresh with them
            foreach (GoapAction a in availableActions)
            {
                a.DoReset();
            }

            // check what actions can run using their checkProceduralPrecondition
            HashSet<GoapAction> usableActions = new HashSet<GoapAction>();
            foreach (GoapAction a in availableActions)
            {
                if (a.CheckProceduralPrecondition(agent))
                    usableActions.Add(a);
            }

            // we now have all actions that can run, stored in usableActions

            // build up the tree and record the leaf nodes that provide a solution to the goal.
            List<Node> leaves = [];

            // build graph
            Node start = new(null, 0, worldState, null);
            bool success = BuildGraph(start, leaves, usableActions, goal);

            if (!success)
            {
                // oh no, we didn't get a plan
                //if (GameManager.Instance.debugLog)
                //    Debug.Log("NO PLAN");
                return null;
            }

            // get the cheapest leaf
            Node cheapest = null;
            foreach (Node leaf in leaves)
            {
                if (cheapest == null)
                    cheapest = leaf;
                else
                {
                    if (leaf.runningCost < cheapest.runningCost)
                        cheapest = leaf;
                }
            }

            // get its node and work back through the parents
            List<GoapAction> result = [];
            Node n = cheapest;
            while (n != null)
            {
                if (n.action != null)
                {
                    result.Insert(0, n.action); // insert the action in the front
                }
                n = n.parent;
            }
            // we now have this action list in correct order

            Queue<GoapAction> queue = new Queue<GoapAction>();
            foreach (GoapAction a in result)
            {
                queue.Enqueue(a);
            }

            // hooray we have a plan!
            return queue;
        }

        /**
         * Returns true if at least one solution was found.
         * The possible paths are stored in the leaves list. Each leaf has a
         * 'runningCost' value where the lowest cost will be the best action
         * sequence.
         */
        private bool BuildGraph(Node parent, List<Node> leaves, HashSet<GoapAction> usableActions, HashSet<KeyValuePair<string, object>> goal)
        {
            bool foundOne = false;

            // go through each action available at this node and see if we can use it here
            foreach (GoapAction action in usableActions)
            {

                // if the Parent state has the conditions for this action's preconditions, we can use it here
                if (InState(action.Preconditions, parent.state))
                {

                    // apply the action's effects to the Parent state
                    HashSet<KeyValuePair<string, object>> currentState = PopulateState(parent.state, action.Effects);
                    //Debug.Log(GoapAgent.prettyPrint(currentState));
                    Node node = new Node(parent, parent.runningCost + action.cost, currentState, action);

                    if (InState(goal, currentState))
                    {
                        // we found a solution!
                        leaves.Add(node);
                        foundOne = true;
                    }
                    else
                    {
                        // not at a solution yet, so test all the remaining actions and branch out the tree
                        HashSet<GoapAction> subset = ActionSubset(usableActions, action);
                        bool found = BuildGraph(node, leaves, subset, goal);
                        if (found)
                            foundOne = true;
                    }
                }
            }

            return foundOne;
        }

        /**
         * Create a subset of the actions excluding the removeMe one. Creates a new set.
         */
        private static HashSet<GoapAction> ActionSubset(HashSet<GoapAction> actions, GoapAction removeMe)
        {
            HashSet<GoapAction> subset = [];
            foreach (GoapAction a in actions)
            {
                if (!a.Equals(removeMe))
                    subset.Add(a);
            }
            return subset;
        }

        /**
         * Check that all items in 'test' are in 'state'. If just one does not match or is not there
         * then this returns false.
         */
        private static bool InState(HashSet<KeyValuePair<string, object>> test, HashSet<KeyValuePair<string, object>> state)
        {
            bool allMatch = true;
            foreach (KeyValuePair<string, object> t in test)
            {
                bool match = false;
                foreach (KeyValuePair<string, object> s in state)
                {
                    if (s.Equals(t))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                    allMatch = false;
            }
            return allMatch;
        }

        /**
         * Apply the stateChange to the currentState
         */
        private static HashSet<KeyValuePair<string, object>> PopulateState(HashSet<KeyValuePair<string, object>> currentState, HashSet<KeyValuePair<string, object>> stateChange)
        {
            HashSet<KeyValuePair<string, object>> state = [];
            // copy the KVPs over as new objects
            foreach (KeyValuePair<string, object> s in currentState)
            {
                state.Add(new KeyValuePair<string, object>(s.Key, s.Value));
            }

            foreach (KeyValuePair<string, object> change in stateChange)
            {
                // if the key exists in the current state, update the Value
                bool exists = false;

                foreach (KeyValuePair<string, object> s in state)
                {
                    if (s.Equals(change))
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    state.RemoveWhere((kvp) => { return kvp.Key.Equals(change.Key); });
                    KeyValuePair<string, object> updated = new KeyValuePair<string, object>(change.Key, change.Value);
                    state.Add(updated);
                }
                // if it does not exist in the current state, add it
                else
                {
                    state.Add(new KeyValuePair<string, object>(change.Key, change.Value));
                }
            }
            return state;
        }

        /**
         * Used for building up the graph and holding the running costs of actions.
         */
        private class Node(GoapPlanner.Node parent, float runningCost, HashSet<KeyValuePair<string, object>> state, GoapAction action)
        {
            public Node parent = parent;
            public float runningCost = runningCost;
            public HashSet<KeyValuePair<string, object>> state = state;
            public GoapAction action = action;
        }

    }
}