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
        
    bool _slideComplete = false;
    bool _slideInvoked = false;
    bool _impactOver = false;

    public override void EnterState()
    {

        Debug.Log("Hello from the Hard Landing State");
        _ctx.MoveLocked = true;
        _ctx.Heading = Vector3.zero;
        if (_ctx.Velocity.magnitude > _ctx.WalkMax)
        {
            //_ctx.HeadingRotated = Vector3.zero;
            _slideInvoked = true;
            _ctx.StartCoroutine(SlideToStop());
        } else
        {
            _ctx.StartCoroutine(SufferImpact());
        }
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        if (_slideComplete && _ctx.Velocity == Vector3.zero) { _ctx.StartCoroutine(SufferImpact());}
        else if (_slideInvoked ){ _ctx.StartCoroutine(SlideToStop());}
    }

    public override void ExitState()
    {
        _slideComplete = false;
        _slideInvoked = false;
        _impactOver = false;
        _ctx.MoveLocked = false; 
    }

    public override void InitializeSubState(){}

    public override void CheckSwitchStates()
    {
        if (_impactOver) {SwitchState(_factory.Idle());}
        // If wait period is over, switch to Idle();
    }

    public IEnumerator SufferImpact()
    {
        Debug.Log("ImpactZone");
        yield return new WaitForSeconds(0.25f);
        // "stand up animation" here
        _impactOver = true;
    }
    
    public IEnumerator SlideToStop()
    {
        while (_ctx.Velocity != Vector3.zero)
        {
            Debug.Log("Sliding");
            yield return null;
        }
        Debug.Log("Slide done?");
        _slideComplete = true;
    }
}
