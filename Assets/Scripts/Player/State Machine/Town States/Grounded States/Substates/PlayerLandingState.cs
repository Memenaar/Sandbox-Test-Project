using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandingState : PlayerGroundedState
{
public PlayerLandingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) {}

    public override void EnterState()
    {
        Debug.Log("Hello from the Landing State");
        GroundedGravity();
        CheckSwitchStates();
    }

    public override void UpdateState(){}

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates(){
        if (_ctx.IsJumpQueued == true)
        {
            SwitchState(_factory.Jump());
        } else
        {
            if (_ctx.Velocity == Vector3.zero)
            {
                SwitchState(_factory.Idle());
            } else if (_ctx.Velocity.magnitude > _ctx.WalkMax)
            {
                SwitchState(_factory.Run());
            } else
            {
                SwitchState(_factory.Walk());
            }
        }
    }
}
