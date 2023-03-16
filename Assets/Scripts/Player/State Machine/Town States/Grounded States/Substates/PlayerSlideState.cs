using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlideState : PlayerGroundedState
{
    public PlayerSlideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory) 
        {
            _parent = _parentState;
        }

    public override void EnterState(){}

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (Mathf.Sign(_ctx.Velocity.x) == Mathf.Sign(_ctx.HeadingRotated.x) && Mathf.Sign(_ctx.Velocity.z) == Mathf.Sign(_ctx.HeadingRotated.z))
        {
            if(_ctx.IsJumpQueued)
            {
                _ctx.IsJumpQueued = false;
                SwitchState(_factory.Jump());
            }
            else if(_ctx.MoveInput == Vector2.zero && _ctx.Velocity.magnitude == 0.0f)
            { 
                SwitchState(_factory.Idle());
            }
            else
            {
                if (_ctx.Velocity.magnitude > _ctx.WalkMax) SwitchState(_factory.Run());
                else SwitchState(_factory.Walk());
            }
        }
    }
}
