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
        InitializeSubState();
    }

    public override void UpdateState(){}

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates(){}
}
