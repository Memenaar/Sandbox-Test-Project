using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerAirborneState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) {}

    private const float HardLandingThreshold = 5f;

    public override void EnterState()
    {
        Debug.Log("Hello from the Fall State");
    }

    public override void UpdateState()
    {
        AirborneGravity();
        CheckSwitchStates();
    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_ctx._charController.isGrounded)  // Change for final ver, char must pass through Falling -> Landing/Hard Landing before becoming grounded.
        {
            if (_ctx.YSpeed > HardLandingThreshold) 
            {
                SwitchState(_factory.HardLanding());
            } else
            {
                SwitchState(_factory.Landing());
            }

        }
    }
}
