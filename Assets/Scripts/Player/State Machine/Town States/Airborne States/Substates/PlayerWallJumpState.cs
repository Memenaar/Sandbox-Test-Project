using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerAirborneState
{
    public PlayerWallJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory)
        {
            _parent = _parentState;
        }

    #region Method Overrides
    public override void EnterState()
    {
        _ctx.CoyoteReady = false;
        _ctx.IsJumpQueued = false;
        WallJump(_ctx.WallNormal);
    }

    public override void UpdateState()
    {

    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_ctx.YSpeed <= 0) 
        {
            SwitchState(_factory.Fall());
        }
    }
    #endregion

    private void WallJump(Vector3 ? horizontalPower = null)
    {
        _ctx.Velocity = Vector3.ClampMagnitude((horizontalPower.Value * _ctx.JumpSpeed), _ctx.JumpSpeed);
        _ctx.YSpeed += (_ctx.JumpSpeed * 0.75f); // Add 3/4 of JumpSpeed to the character's vertical Speed
    }

}
