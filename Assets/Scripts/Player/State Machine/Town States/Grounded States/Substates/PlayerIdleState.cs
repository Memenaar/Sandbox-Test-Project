using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory)
        {
            _parent = _parentState;
        }

    public override void EnterState()
    {
        Debug.Log("Hello from the Idle state");
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        //_ctx.Heading = new Vector3(_ctx.MoveInput.x, 0.0f, _ctx.MoveInput.y);
        // Should have a method that starts a timer, 
        // After a variable amount of time, plays an "impatient" idle animation
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_ctx.MoveInput != Vector2.zero && _ctx.Velocity.magnitude <= _ctx.WalkMax)
        {
            SwitchState(_factory.Walk());
        } else if(_ctx.MoveInput != Vector2.zero && _ctx.Velocity.magnitude > _ctx.WalkMax)
        {
            SwitchState(_factory.Run());
        }
    }
    
}
