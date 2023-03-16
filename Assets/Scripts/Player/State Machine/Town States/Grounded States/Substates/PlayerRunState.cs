using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerGroundedState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory)
        {
            _parent = _parentState;
        }

    public override void EnterState()
    {
        Debug.Log("Hello from the Run State");
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        _ctx.Heading = new Vector3(_ctx.MoveInput.x, 0.0f, _ctx.MoveInput.y); // Create Heading vector from moveinput.
    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (Vector3.Angle(_ctx.Velocity, _ctx.HeadingRotated) >= 90) 
        { 
            SwitchState(_factory.Slide());
        }
        if (_ctx.MoveInput == Vector2.zero && _ctx.Velocity.magnitude == 0.0f)
        {
            SwitchState(_factory.Idle());
        } else if (_ctx.MoveInput != Vector2.zero && _ctx.Velocity.magnitude <= _ctx.WalkMax)
        {
            SwitchState(_factory.Walk());
        }
    }
}
