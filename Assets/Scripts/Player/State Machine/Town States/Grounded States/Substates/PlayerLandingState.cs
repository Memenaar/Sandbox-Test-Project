using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandingState : PlayerGroundedState
{
    public PlayerLandingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory)
        {
            _parent = _parentState;
        }

    public override void EnterState()
    {
        Debug.Log("Hello from the Landing State");
        if (_ctx.IsJumpQueued == true) SwitchState(_factory.Jump());
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Landing State");
    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_ctx.Velocity == Vector3.zero)
        {
            SwitchState(_factory.Idle());
        } else if (_ctx.Velocity.magnitude > _ctx.WalkMax)
        {
            SwitchState(_factory.Run());
        } else
        {
            SwitchState(_factory.Walk());
        }
    }
}
