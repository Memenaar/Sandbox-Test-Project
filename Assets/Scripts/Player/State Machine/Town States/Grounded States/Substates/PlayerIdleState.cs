using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) {}

    public override void EnterState()
    {
        Debug.Log("Hello from the Idle State");
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState(){}

    public override void InitializeSubState(){}
    
}
