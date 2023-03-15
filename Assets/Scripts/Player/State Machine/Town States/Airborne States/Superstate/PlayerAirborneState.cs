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
        Debug.Log("Hello from the Grounded State!");
        AirborneGravity();
        InitializeSubState();

        // If no Substate is set locally 
        if(_localSubState == null) InitializeSubState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        AirborneGravity();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Airborne State");
    }

    public override void InitializeSubState()
    {
        if (_ctx.YSpeed <= 0) SetSubState(_factory.Fall());
    }

    public override void CheckSwitchStates()
    {

    }

    void JumpInputHandler()
    {
        /*if (_coyoteAvailable && (_coyoteTracker + _coyoteTime >= Time.time)) { _jumpState = JumpState.NormalJump; Jump();} // Coyote Time jump                    
        else if (!_jumpQueued) { _jumpQueued = true; _jumpTracker = Time.time;} // Queues a jump for landing*/
    }

    protected void AirborneGravity()
    {
        _ctx.YSpeed += Physics.gravity.y * Time.deltaTime; // Fall at normal rate
        //_ctx.AppliedMove += new Vector3(0, _ctx.YSpeed, 0);
    }
}
