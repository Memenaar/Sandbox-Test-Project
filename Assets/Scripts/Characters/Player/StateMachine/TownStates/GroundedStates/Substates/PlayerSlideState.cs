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

    public override void EnterState()
    {
        Debug.Log("Hello from the Slide state");
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        JumpInputHandler();
    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (Mathf.Sign(_ctx.Velocity.x) == Mathf.Sign(_ctx.HeadingRotated.x) && Mathf.Sign(_ctx.Velocity.z) == Mathf.Sign(_ctx.HeadingRotated.z))
        {
            if(_ctx.IsJumpQueued)
            {
                SwitchState(_factory.Jump());
            }
            else if(_ctx.MoveInput == Vector2.zero && _ctx.Velocity.magnitude == 0.0f)
            { 
                SwitchState(_factory.Idle());
            }
            else
            {
                if (_ctx.Velocity.magnitude > _ctx.PlayerID.WalkMax) SwitchState(_factory.Run());
                else SwitchState(_factory.Walk());
            }
        }
    }

    void JumpInputHandler()
    {
        if (_ctx.IsJumpPressed && !_ctx.NewJumpNeeded && !_ctx.IsJumpQueued){              
            _ctx.IsJumpQueued = true; // Queues a jump for end of Slide
        }
    }
}
