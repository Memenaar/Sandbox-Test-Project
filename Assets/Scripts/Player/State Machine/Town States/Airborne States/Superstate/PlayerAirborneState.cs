using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) 
        {
            _isRootState = true;
        }

    public override void EnterState()
    {
        //Debug.Log("Hello from the Airborne State!");

        _ctx.CoyoteTimer = Time.time;
        _ctx.CharController.stepOffset = 0;

        // If no Substate is set locally
        if(_localSubState == null) InitializeSubState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        JumpInputHandler();
        AirborneGravity();
    }

    public override void ExitState()
    {
    }

    public override void InitializeSubState()
    {
        if (_ctx.IsJumpPressed) SetSubState(_factory.Jump());
        else if (_ctx.YSpeed <= 0 && !_ctx.IsJumpPressed) SetSubState(_factory.Fall());
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.CharController.isGrounded)  // Change for final ver, char must pass through Falling -> Landing/Hard Landing before becoming grounded.
        {
            if (_ctx.YSpeed > _ctx.HardLandingThreshold) 
            {
                SwitchState(_factory.HardLanding());
            } else
            {
                SwitchState(_factory.Landing());
            }
        } else if ((_ctx.CharController.collisionFlags & CollisionFlags.Sides) != 0 && _ctx.JumpWall == true && _ctx.Velocity.magnitude > _ctx.WalkMax)
        {
            if (Mathf.Sign(_ctx.VelocityX) != Mathf.Sign(_ctx.WallNormalX) && Mathf.Sign(_ctx.VelocityZ) != Mathf.Sign(_ctx.WallNormalZ))
            {
                _ctx.JumpWall = false;
                SwitchState(_factory.WallSlide());
            }
        }
    }

    void JumpInputHandler()
    {
        if (_ctx.IsJumpPressed){
            if (_ctx.CoyoteReady && (_ctx.CoyoteTimer + _ctx.CoyoteTime >= Time.time)) { SwitchState(_factory.Jump());} // Coyote Time jump                    
            else if (!_ctx.IsJumpQueued) { _ctx.IsJumpQueued = true; _ctx.JumpTimer = Time.time;} // Queues a jump for landing*/
        }
    }

    protected void AirborneGravity()
    {
        _ctx.YSpeed += Physics.gravity.y * Time.deltaTime; // Fall at normal rate
        //_ctx.AppliedMove += new Vector3(0, _ctx.YSpeed, 0);
    }

}
