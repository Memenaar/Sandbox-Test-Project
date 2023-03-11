using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHardLandingState : PlayerGroundedState
{
public PlayerHardLandingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) {}

    public override void EnterState()
    {
        Debug.Log("Hello from the Hard Landing State");
    }

    public override void UpdateState(){}

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates(){}
}
