using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) 
        {
            _isRootState = true;
        }

    public override void EnterState()
    {
        Debug.Log("Hello from the Grounded state.");
        GroundedGravity();
        _ctx.CharController.stepOffset = _ctx.OriginalStepOffset;

        // If no Substate is set locally 
        if(_localSubState == null) InitializeSubState();
    }

    public override void UpdateState()
    {
        GroundedGravity();
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
        if (_ctx.IsJumpPressed && !_ctx.NewJumpNeeded)
        {
            if ( _ctx.CurrentSubState == _factory.Idle() || _ctx.CurrentSubState == _factory.Walk() || _ctx.CurrentSubState == _factory.Run()) SwitchState(_factory.Jump());
        } else if (!_ctx.CharController.isGrounded) 
        {
            bool _stairMaster = StairMaster();
            if (_stairMaster == false) {SwitchState(_factory.Fall());}
        }
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

    // Helps the player descend steps by snapping the player transform downward if a drop's height is less than the player step offset.
    private bool StairMaster()
    {
        var ray = new Ray(_ctx.transform.position, Vector3.down);
        float maxHeight = _ctx.OriginalStepOffset + 0.1f;

        if (Physics.Raycast(ray, out RaycastHit hitinfo, maxHeight))
        {
                float stairDrop = hitinfo.distance;
                if (stairDrop > 0 && stairDrop <= maxHeight) 
                {
                    _ctx.transform.position = new Vector3(_ctx.transform.position.x, _ctx.transform.position.y - stairDrop, _ctx.transform.position.z); 
                    Physics.SyncTransforms();
                }
                return true;
        }

        return false;
    }
            

}
