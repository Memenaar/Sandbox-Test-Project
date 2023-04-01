using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMantleState : PlayerWallState
{
    public PlayerMantleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, PlayerBaseState _parentState)
        : base (currentContext, playerStateFactory) 
        {
            _parent = _parentState;
        }

    public override void EnterState()
    {
        Debug.Log("Hello from the Mantle State!");
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
    }
}