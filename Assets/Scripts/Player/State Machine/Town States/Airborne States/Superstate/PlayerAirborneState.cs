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
        Debug.Log("Hello from the Airborne State!");

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
            if (Mathf.Abs(_ctx.YSpeed) > _ctx.HardLandingThreshold) 
            {
                SwitchState(_factory.HardLanding());
            } else
            {
                SwitchState(_factory.Landing());
            }
        } else if ((_ctx.CharController.collisionFlags & CollisionFlags.Sides) != 0 && _ctx.Velocity.magnitude > _ctx.WalkMax * 0.5f)
        {
            if (_ctx.JumpWall)
            {
                if (Mathf.Sign(_ctx.VelocityX) != Mathf.Sign(_ctx.WallNormalX) || Mathf.Sign(_ctx.VelocityZ) != Mathf.Sign(_ctx.WallNormalZ))
                {
                    _ctx.JumpWall = false;
                    SwitchState(_factory.WallSlide());
                }
            } else
            {
                if (_ctx.CurrentSubState != _factory.Fall()) {_ctx.YSpeed = 0;}
            }
        }
    }

    void SetHeading()
    {
        _ctx.Heading = _ctx.Velocity;
    }

    void JumpInputHandler()
    {
        if (_ctx.IsJumpPressed && !_ctx.NewJumpNeeded){
            if (_ctx.CoyoteReady && (_ctx.CoyoteTimer + _ctx.CoyoteTime >= Time.time)) { SwitchState(_factory.Jump());} // Coyote Time jump                    
            else if (!_ctx.IsJumpQueued) { _ctx.IsJumpQueued = true; _ctx.JumpTimer = Time.time;} // Queues a jump for landing*/
            //else if (!_ctx.GameInput.TownState.Jump.WasPressedThisFrame() && !_ctx.IsJumpQueued) { _ctx.IsJumpQueued = true; _ctx.JumpTimer = Time.time;}
        }
    }

    protected void AirborneGravity()
    {
        _ctx.YSpeed += Physics.gravity.y * Time.deltaTime; // Fall at normal rate
        //_ctx.AppliedMove += new Vector3(0, _ctx.YSpeed, 0);
    }

}
