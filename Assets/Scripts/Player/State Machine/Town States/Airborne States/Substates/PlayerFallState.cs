using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerAirborneState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory)
        {
            _parent = _parentState;
        }

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

    public override void ExitState()
    {
        Debug.Log("Exiting Fall State");
    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_ctx.CharController.isGrounded)  // Change for final ver, char must pass through Falling -> Landing/Hard Landing before becoming grounded.
        {
            if (_ctx.YSpeed > HardLandingThreshold) 
            {
                //SwitchState(_factory.HardLanding());
            } else
            {
                //SwitchState(_factory.Landing());
            }

        }
    }
}
