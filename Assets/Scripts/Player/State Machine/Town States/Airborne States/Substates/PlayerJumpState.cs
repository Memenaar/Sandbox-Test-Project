using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerAirborneState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory)
        {
            _parent = _parentState;
        }
    
    #region Method Overrides
    public override void EnterState()
    {
        Jump();
        Debug.Log("Hello from the Jump state");
    }

    public override void UpdateState()
    {
        AirborneGravity();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Jump State");
    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_ctx.YSpeed <= 0) 
        {
            //SwitchState(_factory.Fall());
        }
    }
    #endregion

    /*void JumpLogic()
    {
        //if (_slideState == SlideState.Slide && !_jumpQueued) {_jumpQueued = true;} // Queues a jump for end of slide
        //if (_ctx.HeadingRotated == Vector3.zero) {_jumpState = JumpState.StandingJump; Jump();}
        //else if ((_charController.collisionFlags & CollisionFlags.Sides) != 0) {velocity = Vector3.zero; _jumpState = JumpState.StandingJump; Jump();}
        //else { _jumpState = JumpState.NormalJump; Jump();}
        Jump();
    }*/

    private void Jump(Vector3 ? horizontalPower = null)
        {

            // No longer relevant, as WallJump is a seperate state float jumpForce = _jumpState == JumpState.WallJump ? (jumpSpeed * 0.75f) : jumpSpeed;
            //float speedLimit = _moveState == MoveState.Run ? runMax : walkMax;
            /* _ctx.Velocity = Vector3.ClampMagnitude((_ctx.HeadingRotated * _ctx.Velocity.magnitude), runJump);
            if (horizontalPower != null) // If an override is provided
            {
                _ctx.Velocity = Vector3.ClampMagnitude((horizontalPower.Value * speedLimit), runJump); // Set velocity = the input horizontal power value, multiplied by the current SpeedLimit, with a max value = the character's running jump speed.
            } */
            // Move to Falling state _coyoteAvailable = false; // Mark Coyote Time as unavailable.
            _ctx.YSpeed += _ctx.JumpSpeed; // Add the current JumpForce to the player's vertical speed.
            //_ctx.AppliedMoveY += _ctx.YSpeed;
        }
}
