using Godot;
using System.Collections;

public interface FSMState
{

	void Update(FSM fsm, Node2D node);
}

