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
        Debug.Log("Hello from the Grounded State!");
        GroundedGravity();

        // If no Substate is set locally 
        if(_localSubState == null) InitializeSubState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState(){
        //Debug.Log("Exit Grounded State");
    }

    public override void InitializeSubState()
    {
        if (_ctx.MoveInput == Vector2.zero) SetSubState(_factory.Idle());
        Debug.Log("_localSubState = " + _localSubState + ", and it's parent = " + _localSubState.Parent);
        Debug.Log("Current Superstate in Context = " + _ctx.CurrentSuperState + ", and the Substate in Context = " + _ctx.CurrentSubState);
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.IsJumpPressed == true) SwitchState(_factory.Jump());
    }

    protected void GroundedGravity()
    {
        _ctx.YSpeed = _ctx.GroundedGravity;
    }
}
