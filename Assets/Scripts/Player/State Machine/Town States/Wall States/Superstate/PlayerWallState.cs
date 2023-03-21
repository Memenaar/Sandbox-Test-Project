using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallState : PlayerBaseState
{
    public PlayerWallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) 
        {
            _isRootState = true;
        }

    public override void EnterState()
    {
        Debug.Log("Hello from the Wall state");
        if(_localSubState == null) InitializeSubState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState(){}

    public override void InitializeSubState()
    {
        if ((_ctx.CharController.collisionFlags & CollisionFlags.Sides) != 0)
        {
            SetSubState(_factory.WallSlide());
        }
    }

    public override void CheckSwitchStates()
    {
        if (!((_ctx.CharController.collisionFlags & CollisionFlags.Sides) !=0) || _ctx.CharController.collisionFlags == CollisionFlags.Above) // If player becomes free-floating
        {
            _ctx.Velocity = Vector3.zero;
            SwitchState(_factory.Fall());
        }
        else if ((_ctx.CharController.collisionFlags & CollisionFlags.Below) != 0)
        {
            SwitchState(_factory.Landing());
        }
    }
}
