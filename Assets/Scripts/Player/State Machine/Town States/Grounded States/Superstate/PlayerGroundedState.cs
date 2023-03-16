using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    bool fallcheck = false;
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) 
        {
            _isRootState = true;
        }

    public override void EnterState()
    {
        GroundedGravity();

        // If no Substate is set locally 
        if(_localSubState == null) InitializeSubState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        PlayerRun();
    }

    public override void ExitState(){
        //Debug.Log("Exit Grounded State");
    }

    public override void InitializeSubState()
    {
        if (_ctx.MoveInput == Vector2.zero) SetSubState(_factory.Idle());
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.IsJumpPressed == true) SwitchState(_factory.Jump());
        else if (!_ctx.CharController.isGrounded) SwitchState(_factory.Fall());
    }

    protected void GroundedGravity()
    {
        _ctx.YSpeed = _ctx.GroundedGravity;
    }

    private void PlayerRun()
    {
        // Changes player's running state based on Left Shift
        if(_ctx.IsRunPressed && _ctx.Velocity != Vector3.zero)
        {
            _ctx.TurnLerp = Mathf.Lerp(_ctx.TurnLerp, _ctx.RunLerp, Time.deltaTime);
            _ctx.Acceleration = Mathf.Lerp(_ctx.Acceleration, _ctx.RunAccel, Time.deltaTime);
            _ctx.Drag = Mathf.Lerp(_ctx.Drag, _ctx.RunDrag, Time.deltaTime);
            _ctx.JumpSpeed = Mathf.Lerp(_ctx.JumpSpeed, _ctx.RunJump, Time.deltaTime);
        } else {
            _ctx.TurnLerp = Mathf.Lerp(_ctx.TurnLerp, _ctx.WalkLerp, Time.deltaTime);
            _ctx.Acceleration = Mathf.Lerp(_ctx.Acceleration, _ctx.WalkAccel, 0.5f * Time.deltaTime);
            _ctx.Drag = Mathf.Lerp(_ctx.Drag, _ctx.WalkDrag, 0.5f * Time.deltaTime);
            _ctx.JumpSpeed = Mathf.Lerp(_ctx.JumpSpeed, _ctx.WalkJump, Time.deltaTime);
        }
    }

}
