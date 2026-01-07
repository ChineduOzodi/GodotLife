using Godot;
using System.Collections.Generic;

namespace Goap
{

    /// <summary>
    /// Stack-based Finite State Machine.
    /// Push and pop states to the FSM.
    /// 
    /// States should push other states onto the stack 
    /// and pop themselves off.
    /// </summary>
    public partial class FSM
    {

        private readonly Stack<FSMState> stateStack = new Stack<FSMState>();

        public delegate void FSMState(FSM fsm, Node2D node);


        public void Update(Node2D entity)
        {
            stateStack.Peek()?.Invoke(this, entity);
        }

        public void PushState(FSMState state)
        {
            stateStack.Push(state);
        }

        public void PopState()
        {
            stateStack.Pop();
        }
    }
}