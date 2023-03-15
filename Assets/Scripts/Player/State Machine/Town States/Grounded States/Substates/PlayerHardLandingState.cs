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
        Debug.Log("Hello from the Hard Landing State");
    }

    public override void UpdateState(){}

    public override void ExitState()
    {
        Debug.Log("Exiting Hard Landing State");
    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates(){}
}
