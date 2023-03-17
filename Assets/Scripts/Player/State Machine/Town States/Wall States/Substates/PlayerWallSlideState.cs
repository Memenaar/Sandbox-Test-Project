using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideState : PlayerWallState
{
    public PlayerWallSlideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory) 
        {
            _parent = _parentState;
        }

    public override void EnterState()
    {
        _ctx.YSpeed = 0;
        if (_ctx.IsJumpQueued && (_ctx.JumpTimer + _ctx.JumpBuffer > Time.time)) SwitchState(_factory.WallJump());
    }

    public override void UpdateState()
    {
        WallSlideGravity();
        CheckSwitchStates();
    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_ctx.IsJumpPressed == true) SwitchState(_factory.WallJump());
    }

    public void WallSlideGravity()
    {
        _ctx.YSpeed += (Physics.gravity.y * Time.deltaTime) * 0.25f;
        _ctx.Velocity = _ctx.WallNormal * -0.5f;
    }
}
