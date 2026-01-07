using Godot;
using System.Collections;

namespace Goap
{
    public interface IFSMState
    {

        void Update(FSM fsm, Node2D node);
    }
}


