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
        Debug.Log("Hello from the Idle State");
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        //if (_ctx.CharController.isGrounded) Debug.Log("The player is well and truly grounded");
    }

    public override void ExitState()
    {
        Debug.Log("Exit Idle State");
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
