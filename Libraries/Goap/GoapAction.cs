using Godot;
using System.Collections.Generic;

namespace Goap
{
    public abstract class GoapAction
    {


        private readonly HashSet<KeyValuePair<string, object>> preconditions;
        private readonly HashSet<KeyValuePair<string, object>> effects;

        private bool inRange = false;

        /// <summary>
        /// The cost of performing the action.
        /// Figure out a weight that suits the action. 
        /// Changing it will affect what actions are chosen during planning.
        /// </summary>
        public float cost = 1f;

        /// <summary>
        /// An action often has to perform on an object. This is that object. Can be null.
        /// </summary>
        public Node2D target;

        public GoapAction()
        {
            preconditions = [];
            effects = [];
        }

        public void DoReset()
        {
            inRange = false;
            target = null;
            Reset();
        }

        /// <summary>
        /// Reset any variables that need to be reset before planning happens again.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Is the action done?
        /// </summary>
        public abstract bool IsDone();

        /// <summary>
        /// Procedurally check if this action can run. Not all actions
        /// will need this, but some might.
        /// </summary>
        public abstract bool CheckProceduralPrecondition(Node2D agent);

        /// <summary>
        /// Run the action.
        /// Returns True if the action performed successfully or false
        /// if something happened and it can no longer perform. In this case
        /// the action queue should clear out and the goal cannot be reached.
        /// </summary>
        public abstract bool Perform(Node2D agent);

        /// <summary>
        /// Does this action need to be within range of a target game object?
        /// If not then the moveTo state will not need to run for this action.
        /// </summary>
        public abstract bool RequiresInRange();


        /// <summary>
        /// Are we in range of the target?
        /// The MoveTo state will set this and it gets reset each time this action is performed.
        /// </summary>
        public bool IsInRange()
        {
            return inRange;
        }

        public void SetInRange(bool inRange)
        {
            this.inRange = inRange;
        }


        public void AddPrecondition(string key, object value)
        {
            preconditions.Add(new KeyValuePair<string, object>(key, value));
        }


        public void RemovePrecondition(string key)
        {
            KeyValuePair<string, object> remove = default;
            foreach (KeyValuePair<string, object> kvp in preconditions)
            {
                if (kvp.Key.Equals(key))
                    remove = kvp;
            }
            if (!default(KeyValuePair<string, object>).Equals(remove))
                preconditions.Remove(remove);
        }


        public void AddEffect(string key, object value)
        {
            effects.Add(new KeyValuePair<string, object>(key, value));
        }


        public void RemoveEffect(string key)
        {
            KeyValuePair<string, object> remove = default;
            foreach (KeyValuePair<string, object> kvp in effects)
            {
                if (kvp.Key.Equals(key))
                    remove = kvp;
            }
            if (!default(KeyValuePair<string, object>).Equals(remove))
                effects.Remove(remove);
        }


        public HashSet<KeyValuePair<string, object>> Preconditions
        {
            get
            {
                return preconditions;
            }
        }

        public HashSet<KeyValuePair<string, object>> Effects
        {
            get
            {
                return effects;
            }
        }
    }
}