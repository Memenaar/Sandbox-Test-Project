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
        public CharacterController _charController;
        private Transform _navigator;
        private PlayerActions _playerActions;
        private InputHandler _inputHandler;


        [Header("Movement Variables")] // Variables governing character movement and orientation
        public float ySpeed;
        private float originalStepOffset;
        private Vector2 moveInput;
        private Vector3 charDirection;
        public Vector3 velocity;
        public Vector3 priorVelocity;
        public Vector3 heading;
        public Vector3 headingRotated;
        public Vector3 _currentMovement;
        public float turnLerp;
        public float acceleration;
        public float groundDrag;
        public float speedDial;

        [Header("Movement Constants")]
        public float walkMax;
        public float runMax;
        public float walkAccel;
        public float runAccel;
        public float walkDrag;
        public float runDrag;
        public float walkJump;
        public float runJump;
        protected const float WalkLerp = 0.2f;
        protected const float RunLerp = 0.06f;

        [Header("Gravity Variables")]
        private const float _groundedGravity = -0.5f;

        [Header("Jump Variables")] // Variables governing jump motion.
        public bool _isJumpPressed = false;
        public float jumpSpeed;
        public Vector3 _wallNormal;

        [Header("Jump Queueing")]
        private const float _jumpBuffer = 0.15f; // How long prior to landing can the jump input be triggered?
        private float _jumpTracker; // The time the last mid-air jump input was pressed.
        public bool _isJumpQueued; // Is a jump currently queued for landing?

        [Header("Coyote Time")]
        private const float _coyoteTime = .12f; // The player can still jump as if they are on the edge.
        private float _coyoteTracker; // The last time the player became airborne (without jumping)
        private bool _coyoteAvailable = true;

        [Header("Movement States")] // Variables governing character states
        public bool playerGrounded;
        public JumpState _jumpState = JumpState.None;
        public SlideState _slideState = SlideState.None;
        public MoveState _moveState = MoveState.Walk;

        // Temporary Variables
        public Vector3 _appliedMove = Vector3.zero;

        // State Variables
        PlayerBaseState _currentState;
        PlayerStateFactory _states;

    #endregion

    #region Getters & Setters

        // State
        public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; }}

        // Objects & Components
        public Camera Camera { get { return _camera; }}
        public Rigidbody PlayerRb { get { return _playerRb; }}
        public CharacterController CharController { get { return _charController; }}
        public Transform Navigator { get { return _navigator; }}
        public PlayerActions PlayerActions { get { return _playerActions; }}
        
        // Gravity
        public float GroundedGravity { get { return _groundedGravity; }}

        // Movement
        public float YSpeed { get { return ySpeed; } set { ySpeed = value; }}
        private float OriginalStepOffset { get { return originalStepOffset; }}
        private Vector2 MoveInput { get { return moveInput; }}
        private Vector3 CharDirection { get { return charDirection; }}
        public Vector3 Velocity { get { return velocity; } set { velocity = value; }}
        public float TurnLerp { get { return turnLerp; } set { turnLerp = value; }}
        public float Acceleration { get { return acceleration; }}
        public float GroundDrag { get { return groundDrag; }}
        public float SpeedDial { get { return speedDial; }}
        public float AppliedMoveY { get { return _appliedMove.y;} set { _appliedMove.y = value; }}
        public float WalkMax { get { return walkMax;}}

        // Jump Values
        public bool IsJumpPressed { get { return _isJumpPressed; }}
        public float JumpSpeed { get { return jumpSpeed; }}

        // Jump Queuing
        public bool IsJumpQueued { get { return _isJumpQueued; } set { _isJumpQueued = value; }}
        
        // Orientation
        public Vector3 Heading { get { return heading;}}
        public Vector3 HeadingRotated { get { return headingRotated; }}


    #endregion

    void Awake()
        {
            // Setup State
            _states = new PlayerStateFactory(this);
            _currentState = _states.Grounded();
            _currentState.EnterState();

            // Player Action initialization
            _playerActions = new PlayerActions();
            _playerActions.TownState.RunStart.performed += x => RunPressed();
            _playerActions.TownState.RunFinish.performed += x => RunReleased();
            _playerActions.TownState.Jump.started += onJump;
            _playerActions.TownState.Jump.canceled += onJump;

            InitializeMovement();

            // Value Initialization
            groundDrag = walkDrag;
            acceleration = walkAccel;
            turnLerp = WalkLerp;
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
        _currentState.UpdateState();
        MoveChar();
    }

    void FixedUpdate()
    {

    }

    protected void InitializeMovement() // Initialize needed game objects, components, and variables.
    {
        _camera = Camera.main;
        _playerRb = GetComponent<Rigidbody>();
        _navigator = gameObject.transform.Find("Navigator").GetComponent<Transform>();
        _charController = GetComponent<CharacterController>();
        originalStepOffset = _charController.stepOffset;
    }

    private void onJump(InputAction.CallbackContext context)
    { 
        _isJumpPressed = context.ReadValueAsButton();
        Debug.Log("Is jump pressed? " + _isJumpPressed);
    }
    
    private void RunPressed() {_moveState = MoveState.Run;}
    private void RunReleased() {_moveState = MoveState.Walk;}

    private void RotateNavigator()
    {
        if (_slideState == SlideState.None)
        {
            if (_charController.isGrounded)
            {
                if (headingRotated != Vector3.zero)
                {
                    _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), turnLerp);
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
            if (headingRotated!= Vector3.zero) _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), turnLerp); 
        }
    }

    private void MoveChar()
        {
            float speedLimit = _moveState == MoveState.Run ? runMax : walkMax;
            float dragFactor = _moveState == MoveState.Run ? runDrag : walkDrag;
            float accelFactor = _moveState == MoveState.Run ? runAccel : walkAccel;
            if (headingRotated != Vector3.zero)
            {
                if(_charController.isGrounded)
                {
                    velocity = Vector3.MoveTowards(velocity, new Vector3(speedLimit * headingRotated.x, 0.0f, speedLimit * headingRotated.z), acceleration * Time.deltaTime);
                }
                else if(!_charController.isGrounded && _jumpState == JumpState.StandingJump)
                {
                    velocity = Vector3.MoveTowards(velocity, new Vector3(speedLimit * headingRotated.x, 0.0f, speedLimit * headingRotated.z), acceleration * Time.deltaTime);
                }
                else
                {
                    velocity = Vector3.MoveTowards(velocity, new Vector3(velocity.x, velocity.y, velocity.z), acceleration * Time.deltaTime);
                }
            }
            else
            {
                velocity = Vector3.MoveTowards(velocity, Vector3.zero, groundDrag * Time.deltaTime);
            }
            
           
            Vector3 _appliedMove = velocity;
            //move = AdjustVelocityToSlope(move);
            _appliedMove.y += ySpeed;

            _charController.Move(_appliedMove * Time.deltaTime);
        
            RotateNavigator();
        }
}
