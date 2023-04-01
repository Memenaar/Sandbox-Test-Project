using UnityEngine;

public abstract class PlayerBaseState
{
    protected bool _isRootState = false;
    protected PlayerStateMachine _ctx;
    protected PlayerBaseState _parent;
    public PlayerBaseState Parent {get { return _parent; }}
    protected PlayerStateFactory _factory;
    protected PlayerBaseState _localSuperState;
    protected PlayerBaseState _localSubState;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _ctx = currentContext;
        _factory = playerStateFactory;
        _parent = null;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchStates();

    public abstract void InitializeSubState();

    public void UpdateStates()
    {
        UpdateState();
        if (_localSubState != null) {
            _localSubState.UpdateStates();
        }
    }

    public void EnterStates()
    {
        EnterState();
        if (_localSubState != null)
        {
            _localSubState.EnterStates();
        }
    }

    public void ExitStates()
    {
        ExitState();
        if (_localSubState != null)
        {
            _localSubState.ExitStates();
        }
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        if (_ctx.CurrentSuperState == newState.Parent) // If the new State belongs to the current SuperState
        {
            //Debug.Log(newState + "'s parent state " + newState.Parent + " is a match for current SuperState " + _ctx.CurrentSuperState);
            if (_ctx.CurrentSubState != null) _ctx.CurrentSubState.ExitState(); // Exit the current SubState
            
            newState.EnterState(); // Enter the new SubState

            _ctx.CurrentSubState = newState; // Switch the CurrentSubState to the new state.

            _ctx.CurrentSuperState.SetSubState(newState); // Run the SetSubState method in the current SuperState, using the new substate as a parameter.

        } else
        {
            //Debug.Log(newState + "'s parent state " + newState.Parent + " is NOT a match for current SuperState " + _ctx.CurrentSuperState);
            _ctx.CurrentSuperState.ExitStates(); // Exit current Super and Sub states.
            _ctx.CurrentSuperState = newState.Parent; // Set the current Superstate to the Parent of the incoming Substate.
            _ctx.CurrentSuperState.SetSubState(newState); // Set the current Substate in the newly updated Superstate.
            _ctx.CurrentSuperState.EnterStates(); // Run Enter State in the newly updated Super and Sub states.
        }
    }

    // Lower-order functionality of Switch States. Only sets the _localSuperState parameter within the Substate.
    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        _localSuperState = newSuperState;
    }

    // Lower-order functionality of Switch States. Only sets the _localSubState parameter within the Superstate, then creates a reference from the new substate to the superstate.
    protected void SetSubState(PlayerBaseState newSubState)
    {
            _localSubState = newSubState;
            _ctx.CurrentSubState = _localSubState;
            newSubState.SetSuperState(this); // Runs the Set Super State method in the new SubState, using the current SuperState as a parameter.
    }

}
