using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SpriteController;

public class PlayerStateMachine : MonoBehaviour
{
    #region Variable Declarations
        [Header("Objects & Components")] // Variables containing objects and components
        private Camera _camera;
        private Rigidbody _playerRb;
        private CharacterController _charController;
        private Transform _navigator;
        private PlayerActions _playerActions;
        private InputHandler _inputHandler;

        [Header("Input")] // Variables that read and govern player inputs
        private bool _isJumpPressed = false;
        private bool _isRunPressed = false;
        public Vector2 _moveInput;

        [Header("Player State")] // Variables that interface with the Player State Machine.
        public Character _activeChar = Character.Ivy;
        private PlayerBaseState _currentSuperState;
        private PlayerBaseState _currentSubState;
        private PlayerStateFactory _states;

        [Header("Gravity")]
        private const float groundedgravity = -0.5f;
        private const float hardlandingthreshold = 5f;

        [Header("Movement Constants")]
        public float walkmax;
        public float walkaccel;
        public float walkdrag;
        public float walkjump;
        public const float walklerp = 0.2f;
        public float runmax;
        public float runaccel;
        public float rundrag;
        public float runjump;
        public const float runlerp = 0.06f;
        public float speedtuner;

        [Header("Movement Variables")] // Variables governing character movement and orientation
        public float acceleration;
        public float drag;
        public float turnLerp;
        public float jumpSpeed;
        public float ySpeed;
        private float originalStepOffset;
        public Vector3 velocity;
        public Vector3 priorVelocity;

        [Header("Orientation")] // Variables governing player orientation
        private Vector3 charDirection;
        public Vector3 _heading;
        public Vector3 _headingRotated;

        [Header("Collisions")] // Variables governing jump motion.
        public Vector3 _wallNormal;
        public bool _jumpWall;
        private ControllerColliderHit _lastHit;

        [Header("Jump Queueing")]
        private const float jumpbuffer = 0.15f; // How long prior to landing can the jump input be triggered?
        private float _jumpTimer; // The time the last mid-air jump input was pressed.
        public bool _isJumpQueued; // Is a jump currently queued for landing?

        [Header("Coyote Time")]
        private const float coyotewindow = .12f; // The player can still jump as if they are on the edge.
        private float _coyoteTimer; // The last time the player became airborne (without jumping)
        private bool _coyoteReady = true;

        //[Header("Movement States")] // Variables governing character states
        //public bool playerGrounded;
        //public JumpState _jumpState = JumpState.None;
        //public SlideState _slideState = SlideState.None;
        //public MoveState _moveState = MoveState.Walk;

    #endregion

    #region Getters & Setters
        // Objects & Components
        public Camera Camera { get { return _camera; }}
        public Rigidbody PlayerRb { get { return _playerRb; }}
        public CharacterController CharController { get { return _charController; }}
        public Transform Navigator { get { return _navigator; }}
        public PlayerActions PlayerActions { get { return _playerActions; }}
        public PlayerStateFactory Factory { get { return _states; }}

        // Input
        public bool IsJumpPressed { get { return _isJumpPressed; }}
        public bool IsRunPressed { get { return _isRunPressed; }}
        public Vector2 MoveInput { get { return _moveInput; }}

        // State
        public PlayerBaseState CurrentSuperState { get { return _currentSuperState; } set { _currentSuperState = value; }}
        public PlayerBaseState CurrentSubState { get { return _currentSubState; } set { _currentSubState = value; }}
        
        // Gravity
        public float GroundedGravity { get { return groundedgravity; }}
        public float HardLandingThreshold {get { return hardlandingthreshold; }}

        // MovementConstants
        public float WalkMax { get { return walkmax; }}
        public float WalkAccel { get { return walkaccel;}}
        public float WalkDrag { get { return walkdrag; }}
        public float WalkJump { get { return walkjump; }}
        public float WalkLerp { get { return walklerp; }}
        public float RunMax { get { return runmax; }}
        public float RunAccel { get { return runaccel; }}
        public float RunDrag { get { return rundrag; }}
        public float RunJump { get { return runjump; }}
        public float RunLerp { get { return runlerp; }}
        public float SpeedTuner { get { return speedtuner; }}

        // Movement Variables
        public float Acceleration { get { return acceleration; } set { acceleration = value; }}
        public float Drag { get { return drag; } set { drag = value; }}
        public float TurnLerp { get { return turnLerp; } set { turnLerp = value; }}
        public float JumpSpeed { get { return jumpSpeed; } set { jumpSpeed = value; }}
        public float YSpeed { get { return ySpeed; } set { ySpeed = value; }}
        public float OriginalStepOffset { get { return originalStepOffset; }}
        public float VelocityX { get { return velocity.x; } set { velocity.x = value; }}
        public float VelocityZ { get { return velocity.z; } set { velocity.z = value; }}
        public Vector3 Velocity { get { return velocity; } set { velocity = value; }}
        public Vector3 PriorVelocity { get { return priorVelocity; } set { priorVelocity = value; }}
        
        // Orientation
        public Vector3 CharDirection { get { return charDirection; }}
        public Vector3 Heading { get { return _heading;} set { _heading = value; }}
        public Vector3 HeadingRotated { get { return _headingRotated; }}

        // Collisions
        public bool JumpWall { get { return _jumpWall; } set { _jumpWall = value; }}
        public float WallNormalX {get { return _wallNormal.x; } set { _wallNormal.x = value;}}
        public float WallNormalZ {get { return _wallNormal.z; } set { _wallNormal.z = value;}}
        public Vector3 WallNormal { get { return _wallNormal; } set { _wallNormal = value; }}
        public ControllerColliderHit LastHit {get { return _lastHit; }}

        // Jump Queuing
        public float JumpBuffer { get { return jumpbuffer; }}
        public bool IsJumpQueued { get { return _isJumpQueued; } set { _isJumpQueued = value; }}
        public float JumpTimer { get { return _jumpTimer; } set { _jumpTimer = value; }}

        // Coyote Time
        public float CoyoteTime { get { return coyotewindow; }}
        public float CoyoteTimer { get {return _coyoteTimer; } set { _coyoteTimer = value; }}
        public bool CoyoteReady { get { return _coyoteReady; } set { _coyoteReady = value; }}

    #endregion

    void Awake()
        {
            InitializeMovement();
            InitializeStates();

            // Player Action initialization
            _playerActions = new PlayerActions();
            _playerActions.TownState.RunStart.performed += x => RunPressed();
            _playerActions.TownState.RunFinish.performed += x => RunReleased();
            _playerActions.TownState.Jump.started += onJump;
            _playerActions.TownState.Jump.canceled += onJump;
            _playerActions.TownState.Movement.performed += x => OnMove();
            _playerActions.TownState.Movement.canceled += x => OnMove();

            // Value Initialization
            drag = walkdrag;
            acceleration = walkaccel;
            turnLerp = walklerp;
        }

    void OnEnable()
    {
        _playerActions.TownState.Enable();
    }
    
    void OnDisable()
    {
        _playerActions.TownState.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        _currentSuperState.UpdateStates();
        InputToHeading();
        MoveChar();
        _lastHit = null;
        //_wallNormal = Vector3.zero;
    }

    void FixedUpdate()
    {

    }

    protected void InitializeStates() // Initialize needed game objects, components, and variables.
    {
        // Set the player's superstate to Grounded to start for simplicity.
        _currentSuperState = _states.Grounded();

        // Enter the selected superstate
        _currentSuperState.EnterStates();
    }

    protected void InitializeMovement() // Initialize needed game objects, components, and variables.
    {
        _camera = Camera.main;
        _playerRb = GetComponent<Rigidbody>();
        _navigator = gameObject.transform.Find("Navigator").GetComponent<Transform>();
        _charController = GetComponent<CharacterController>();
        originalStepOffset = _charController.stepOffset;
        
        // Initialize Player State Factory
        _states = new PlayerStateFactory(this);
    }

    private void onJump(InputAction.CallbackContext context)
    { 
        _isJumpPressed = context.ReadValueAsButton();
    }
    
    private void RunPressed() {_isRunPressed = true;}
    private void RunReleased() {_isRunPressed = false;}
    private void OnMove() {_moveInput = _playerActions.TownState.Movement.ReadValue<Vector2>();}

    private void RotateNavigator()
    {
        if (_currentSubState == _states.Slide())
        {
            if (_charController.isGrounded)
            {
                if (_headingRotated != Vector3.zero)
                {
                    _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(_headingRotated.normalized), turnLerp);
                }
            } else
            {
                if (velocity != Vector3.zero)
                {
                    _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(velocity.normalized), turnLerp);
                }
            }
        }else 
        { 
            if (_headingRotated!= Vector3.zero) _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(_headingRotated.normalized), turnLerp); 
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
            print("Any combination of collisions EXCEPT touching ground only.");
            _wallNormal = new Vector3(hit.normal.x, hit.normal.y, hit.normal.z); // Get the x and z axes of the collision's normal. Omission of y axis prevents undue up/downward momentum in wall jump.
            _lastHit = hit;
            if (hit.gameObject.CompareTag("WallJump")) _jumpWall = true;
            else _jumpWall = false; 
    }

    private void MoveChar()
    {
        float speedLimit = _isRunPressed ? runmax : walkmax;
        float dragFactor = _isRunPressed ? rundrag : walkdrag;
        float accelFactor = _isRunPressed ? runaccel : walkaccel;
        if (_headingRotated != Vector3.zero)
        {
            if(_currentSuperState == _states.Grounded())
            {
                velocity = Vector3.MoveTowards(velocity, new Vector3(speedLimit * _headingRotated.x, 0.0f, speedLimit * _headingRotated.z), acceleration * Time.deltaTime);
            }
            else
            {
                velocity = Vector3.MoveTowards(velocity, new Vector3(velocity.x, velocity.y, velocity.z), acceleration * Time.deltaTime);
            }
        }
        else
        {
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, drag * Time.deltaTime);
        }
        
        
        Vector3 _appliedMove = velocity;
        _appliedMove = AdjustVelocityToSlope(_appliedMove);
        _appliedMove.y += ySpeed;

        _charController.Move(_appliedMove * Time.deltaTime);
    
        RotateNavigator();
    }

    private void InputToHeading(bool clampInput = false) // Uses a default value of false for clampInput if one is not provided.
    {
        _headingRotated = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * _heading; // Rotate heading relative to camera direction       
        _headingRotated.Normalize(); // Normalize the rotated heading.
        if (clampInput) {_headingRotated = _headingRotated * 0.25f;} // Reduces effect of input to 1/4 is clampInput is true.
    }

    private Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        var ray = new Ray(transform.position, Vector3.down); // Cast a ray downward from the player's transform

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.2f)) // Generatese hitInfo if the raycast hits something at a max of 0.2f distance
        {
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal); // Get the rotation of any slope under the player from the raycast's normal
            var adjustedVelocity = slopeRotation * velocity; // Adjust player velocity to match the rotation of the slope

            if (adjustedVelocity.y < 0) // if the adjust velocity is less than 0 (ie: the slope is downward)
            {
                return adjustedVelocity; // then return the adjust velocity
            }
        }

        return velocity; // otherwise return the original velocity.
    }

}
