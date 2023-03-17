using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerAirborneState
{
    bool _wallTouch = false;
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory)
        {
            _parent = _parentState;
        }
    
    #region Method Overrides
    public override void EnterState()
    {
        _ctx.CoyoteReady = false;
        _ctx.IsJumpQueued = false;
        Jump();
    }

    public override void UpdateState()
    {
        AirborneGravity();
        if ((_ctx.CharController.collisionFlags & CollisionFlags.Above) != 0){ HeadBump(); }
        CheckSwitchStates();
    }

    public override void ExitState()
    {
    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_ctx.YSpeed <= 0) 
        {
            SwitchState(_factory.Fall());
        }
    }
    #endregion

    private void Jump(Vector3 ? horizontalPower = null)
    {
        //_ctx.Velocity = Vector3.ClampMagnitude((_ctx.HeadingRotated * _ctx.Velocity.magnitude), _ctx.RunJump);
        _ctx.YSpeed += _ctx.JumpSpeed; // Add the current JumpForce to the player's vertical speed.
    }

    private void HeadBump() // Ensures if you hit your head on something while jumping you don't hang under it until gravity takes effect.
    {
        _ctx.Velocity += _ctx.Velocity * -5 * Time.deltaTime; // Reduce velocity by -5* TdeltaT
        _ctx.YSpeed += _ctx.YSpeed * -1.5f; // Multiply ySpeed by 1.5 and invert.
        Debug.Log("Headbump");
    }
}
