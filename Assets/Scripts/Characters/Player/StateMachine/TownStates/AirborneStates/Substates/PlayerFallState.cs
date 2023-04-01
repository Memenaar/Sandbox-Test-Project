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

    public override void EnterState()
    {
        Debug.Log("Hello from the Fall state");
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates(){}
}
