using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerAirborneState
{
    public PlayerWallJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base (currentContext, playerStateFactory) {}

    #region Method Overrides
    public override void EnterState()
    {
        WallJumpLogic();
    }

    public override void UpdateState()
    {

    }

    public override void ExitState(){}

    public override void InitializeSubState(){}

    public override void CheckSwitchStates(){}
    #endregion

    void WallJumpLogic()
    {
       /*Jump(_wallNormal); */
    }

}
