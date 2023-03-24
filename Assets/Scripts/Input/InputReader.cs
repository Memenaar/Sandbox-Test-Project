using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]

public class InputReader : DescriptionBaseSO, GameInput.ICameraActions, GameInput.ITownStateActions, GameInput.IDungeonStateActions
{
    /*
    [Space]
    [SerializeField] private GameStateSO _gameStateManager; */

    // Camera
    public event UnityAction<Vector2, bool> CameraRotateEvent = delegate { };
    public event UnityAction<Vector2, bool> CameraZoomEvent = delegate { };
    public event UnityAction EnableMouseControlCameraEvent = delegate { };
    public event UnityAction DisableMouseControlCameraEvent = delegate { };

    // Insert other Action Map events here

    private GameInput _gameInput;
    public GameInput GameInput { get { return _gameInput; }}

    private void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new GameInput();

            _gameInput.Camera.SetCallbacks(this);
            _gameInput.TownState.SetCallbacks(this);
            _gameInput.DungeonState.SetCallbacks(this);
        }
    }

    private void OnDisable()
    {
        DisableAllInput();
    }

    // ICameraActions interface members
    public void OnCameraRotation(InputAction.CallbackContext context)
    {
        CameraRotateEvent.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
    }

    public void OnCameraZoom(InputAction.CallbackContext context)
    {
        CameraZoomEvent.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
    }

    public void OnMouseControlCamera(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            EnableMouseControlCameraEvent.Invoke();
        
        if (context.phase == InputActionPhase.Canceled)
            DisableMouseControlCameraEvent.Invoke();
    }

    // ITownStateActions interface members
    public void OnJump(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnRunStart(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnRunFinish(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    // IDungeonStateActions interface members
        // Currently Empty

    // Unsorted
    private bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";

    // Enable/Disable Methods for various action maps
    public void DisableAllInput()
    {
        _gameInput.Camera.Disable();
        _gameInput.TownState.Disable();
        _gameInput.DungeonState.Disable();
    }

    public void EnableTownInput()
    {
        _gameInput.Camera.Enable();
        _gameInput.TownState.Enable();
        _gameInput.DungeonState.Disable();
    }

    public void EnableDungeonInput()
    {
        _gameInput.Camera.Enable();
        _gameInput.DungeonState.Enable();
        _gameInput.TownState.Disable();
    }
}
