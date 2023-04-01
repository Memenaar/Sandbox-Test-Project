using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractState : PlayerGroundedState
{
    public PlayerInteractState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory) 
        {
            _parent = _parentState;
        }

    public override void EnterState()
    {
        _ctx.Velocity = Vector3.zero;
        _ctx.Heading = Vector3.zero;
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (!_ctx.PlayerInteraction)
        {
            SwitchState(_factory.Idle());
        }
    }
}
