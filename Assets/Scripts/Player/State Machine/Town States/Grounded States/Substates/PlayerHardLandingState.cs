using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHardLandingState : PlayerGroundedState
{
    public PlayerHardLandingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory)
        {
            _parent = _parentState;
        }

    public override void EnterState()
    {
        //Debug.Log("Hello from the Hard Landing State");
        // if velocity.magnitude > walkMax, lock input & slide in direction of velocity for x seconds.
        // else lock input for x seconds
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        //Debug.Log("Exiting Hard Landing State");
    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        // If wait period is over, switch to Idle();
    }
}
