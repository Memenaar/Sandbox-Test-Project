using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpriteController
{
    public class CharMovement : MonoBehaviour
    {

        [Header("Objects & Components")] // Variables containing objects and components
        private Camera _camera;
        private Rigidbody _playerRb;
        private CharacterController _charController;
        private Transform _navigator;
        private PlayerActions _playerActions;

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
        protected const float walkLerp = 0.2f;
        protected const float runLerp = 0.06f;

        [Header("Jump Variables")] // Variables governing jump motion.
        public float jumpSpeed;
        public Vector3 _wallNormal;

        [Header("Jump Queueing")]
        private const float _jumpBuffer = 0.15f; // How long prior to landing can the jump input be triggered?
        private float _jumpTracker; // The time the last mid-air jump input was pressed.
        public bool _jumpQueued; // Is a jump currently queued for landing?

        [Header("Coyote Time")]
        private const float _coyoteTime = .12f; // The player can still jump as if they are on the edge.
        private float _coyoteTracker; // The last time the player became airborne (without jumping)
        private bool _coyoteAvailable = true;

        [Header("Movement States")] // Variables governing character states
        public bool playerGrounded;
        public JumpState _jumpState = JumpState.None;
        public SlideState _slideState = SlideState.None;
        public MoveState _moveState = MoveState.Walk;

        void Awake()
        {
            InitializeMovement();

            // Value Initialization
            groundDrag = walkDrag;
            acceleration = walkAccel;
            turnLerp = walkLerp;


            // Player Action initialization
            _playerActions = new PlayerActions();
            _playerActions.WorldGameplay.RunStart.performed += x => RunPressed();
            _playerActions.WorldGameplay.RunFinish.performed += x => RunReleased();
            _playerActions.WorldGameplay.Jump.performed += x => JumpLogic();
        }
        
        void OnEnable()
        {
            _playerActions.WorldGameplay.Enable();
        }
        void OnDisable()
        {
            _playerActions.WorldGameplay.Disable();
        }

        // Update is called once per frame
        void Update()
        {
            if (_playerActions.WorldGameplay.Jump.WasPressedThisFrame())
            {
                // Skip ahead to movement if Jump was pressed this frame.
                if (_charController.isGrounded) 
                {
                    if (!playerGrounded) BecomeGrounded();
                }
                else
                {
                    if (playerGrounded) BecomeAirborne(); // Use the BecomeAirborne method.
                }
            }
            else
            {
                moveInput = _playerActions.WorldGameplay.Movement.ReadValue<Vector2>();
               // Gets movement input
                if (_charController.isGrounded) // Executed if player's character controller is grounded.
                {
                    ySpeed = -0.5f; // ySpeed must be reset every frame the character is on the ground in order to allow them to move under simulated gravity.

                    if (!playerGrounded) // If boolean playerGrounded is set to False
                    {
                        // Use the BecomeGrounded method
                        BecomeGrounded();
                    }

                    SlideCheck();
                    InputToHeading(); // Converts player input into a heading for the player character.
                    PlayerRun(); // Checks whether the player is running 
                } 
                else // Executed if player's character controller is airborne.
                {
                    if (playerGrounded) BecomeAirborne(); // Use the BecomeAirborne method.
                    AirborneBehaviour();
                    if (_jumpState == JumpState.StandingJump && _moveState != MoveState.Locked) InputToHeading(true);
                
                }  
            }
            
            MoveChar();
            
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _wallNormal = new Vector3(hit.normal.x, 0.0f, hit.normal.z);
            if (_charController.isGrounded) // If Char is grounded 
            {
                if (_charController.collisionFlags == CollisionFlags.Below) // If character is ONLY touching ground
                {

                }
                else if ((_charController.collisionFlags & CollisionFlags.Sides) != 0) // If character is touching sides
                {
                    velocity = Vector3.ClampMagnitude(velocity, 1); // Clamp magnitude of the character's velocity to walkMax
                    Debug.Log("Wall collision");
                }

            }
            else // If Char is not grounded
            {
                if ((_charController.collisionFlags & CollisionFlags.Above) != 0) // If character is touching the ceiling
                {
                    HeadBump();
                }
                else if ((_charController.collisionFlags & CollisionFlags.Sides) != 0) // If character is touching sides
                {

                    if (velocity.magnitude > walkMax && ySpeed > 0 && hit.gameObject.CompareTag("WallJump") && _jumpState != (JumpState.StandingJump | JumpState.None))
                    {
                        _wallNormal.y = 0.0f;
                        ySpeed = 0;
                        _jumpState = JumpState.None;
                        _slideState = SlideState.WallSlide;
                    }
                    else if (_jumpState != JumpState.None)
                    {
                        _jumpState = JumpState.StandingJump;
                    }
                }
            }
        }

        /* METHODS BELOW THIS POINT */
        

        // Fixes slope bounce
        private Vector3 AdjustVelocityToSlope(Vector3 velocity)
        {
            var ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.2f))
            {
                var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                var adjustedVelocity = slopeRotation * velocity;

                if (adjustedVelocity.y < 0)
                {
                    return adjustedVelocity;
                }
            }

            return velocity;
        }

        // Governs player movement in the air.
        private void AirborneBehaviour()
        {
            if (_slideState == SlideState.WallSlide) // If player is in the wall slide state
            {
                if ((_charController.collisionFlags & CollisionFlags.Sides) != 0) // As long as the player is still in collision with a wall
                {
                    ySpeed += (Physics.gravity.y * Time.deltaTime) * 0.25f; // Apply gravity at half the regular rate
                    velocity = _wallNormal * -0.5f; // And set the player's velocity to = 0.5f in the direction of wall contact.
                }
                else // If wall collision ceases
                {
                    _slideState = SlideState.None; // Remove Wall Slide state
                    ySpeed += Physics.gravity.y * Time.deltaTime; // Fall at normal rate
                }
            }
            else // If player is not in the wall slide state
            {
                ySpeed += Physics.gravity.y * Time.deltaTime; // Apply gravity at the regular rate
            }
        }

        private void BecomeAirborne() // Applies changes that need to occur once the character leaves ground
        {
            _coyoteTracker = Time.time; // Set the last "leftGround" time to the current time.
            _charController.stepOffset = 0.0f; // Set the character controller's step offset to 0. Prevents characters mantling mid-air.
            playerGrounded = false;
        }

        private void BecomeGrounded() // Applies changes that need to occur when the player becomes grounded.
        {
            // Check whether a jump is buffered.
            if (_jumpQueued && (_jumpTracker + _jumpBuffer > Time.time))
            {
                _jumpState = JumpState.NormalJump;
                Jump();
                _jumpQueued = false;
            } else {
                _charController.stepOffset = originalStepOffset;
                playerGrounded = true;
                _coyoteAvailable = true;
                _jumpQueued = false;
                _jumpState = JumpState.None;
                if (_slideState != SlideState.None) _slideState = SlideState.None;

            }
        }

        // Ensures if you hit your head on something while jumping you don't hang under it until gravity takes effect.
        private void HeadBump()
        {
                velocity.x += velocity.x * -10 * Time.deltaTime;
                velocity.z += velocity.z * -10 * Time.deltaTime;
                ySpeed += ySpeed * -2f;
        }
        
        protected void InitializeMovement()
        {
            _camera = Camera.main;
            _playerRb = GetComponent<Rigidbody>();
            _navigator = gameObject.transform.Find("Navigator").GetComponent<Transform>();
            _charController = GetComponent<CharacterController>();
            originalStepOffset = _charController.stepOffset;
        }

        // Converts player input into a heading for the player character
        private void InputToHeading(bool clampInput = false) // Uses a default value of false for clampInput if one is not provided.
        {
                heading = new Vector3(moveInput.x, 0.0f, moveInput.y);
                headingRotated = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * heading;            
                headingRotated.Normalize();
                if (clampInput) {headingRotated = headingRotated * 0.25f;} // Reduces input's effect to 1/4 is clampInput is true.

        }

        private void JumpLogic() //Determines which kind of jump to use.
        { 
            if (_jumpState == JumpState.None)
            {
                if(_charController.isGrounded)
                {
                    if (_slideState == SlideState.Slide && !_jumpQueued) {_jumpQueued = true;} // Queues a jump for end of slide
                    else if (headingRotated == Vector3.zero) {_jumpState = JumpState.StandingJump; Jump();}
                    else if ((_charController.collisionFlags & CollisionFlags.Sides) != 0) {velocity = Vector3.zero; _jumpState = JumpState.StandingJump; Jump();}
                    else { _jumpState = JumpState.NormalJump; Jump();} // Regular jump
                }
                else
                {
                    if (_coyoteAvailable && (_coyoteTracker + _coyoteTime >= Time.time)) { _jumpState = JumpState.NormalJump; Jump();} // Coyote Time jump                    
                    else if (!_jumpQueued) { _jumpQueued = true; Debug.Log("Pebis."); _jumpTracker = Time.time;} // Queues a jump for landing
                    else if (_slideState == SlideState.WallSlide) {_jumpState = JumpState.WallJump; _slideState = SlideState.None; Jump(_wallNormal); Debug.Log(_wallNormal);} // Wall Jump
                }
            }
            else if (!_coyoteAvailable && !_jumpQueued) { _jumpQueued = true; _jumpTracker = Time.time;} // Queues a jump for landing
            
        }

        private void Jump(Vector3 ? horizontalPower = null)
        {

            float jumpForce = _jumpState == JumpState.WallJump ? (jumpSpeed * 0.75f) : jumpSpeed;
            float speedLimit = _moveState == MoveState.Run ? runMax : walkMax;
            velocity = Vector3.ClampMagnitude((headingRotated * velocity.magnitude), runJump);
            if (horizontalPower == null) // If no override is provided
            {
                if (velocity != Vector3.zero) 
                {
                    velocity = Vector3.ClampMagnitude(velocity = Vector3.zero + (headingRotated * velocity.magnitude * jumpForce), speedLimit);
                    RotateNavigator(headingRotated.normalized);
                }
                Debug.Log("Jump without horizontal override");
            }
            else // If an override for horizontal jump power is provided 
            {
                velocity = Vector3.ClampMagnitude((horizontalPower.Value * speedLimit), runJump); Debug.Log(velocity);
                RotateNavigator(velocity);
                Debug.Log("Jump with horizontal override");
            }
            _coyoteAvailable = false; 
            ySpeed += jumpForce;
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
            
           
            Vector3 move = velocity;
            move = AdjustVelocityToSlope(move);
            move.y += ySpeed;

            //Debug.Log(move);

            _charController.Move(move * Time.deltaTime);
        
            RotateNavigator();
        }

        private void PlayerRun()
        {
            // Changes player's running state based on Left Shift
            if(_moveState == MoveState.Run && velocity != Vector3.zero && _charController.isGrounded)
            {
                turnLerp = Mathf.Lerp(turnLerp, runLerp, Time.deltaTime);
                acceleration = Mathf.Lerp(acceleration, runAccel, Time.deltaTime);
                groundDrag = Mathf.Lerp(groundDrag, runDrag, Time.deltaTime);
                jumpSpeed = Mathf.Lerp(jumpSpeed, runJump, Time.deltaTime);
            } else {
                if (_charController.isGrounded)
                {
                    turnLerp = Mathf.Lerp(turnLerp, walkLerp, Time.deltaTime);
                    acceleration = Mathf.Lerp(acceleration, walkAccel, 0.5f * Time.deltaTime);
                    groundDrag = Mathf.Lerp(groundDrag, walkDrag, 0.5f * Time.deltaTime);
                    jumpSpeed = Mathf.Lerp(jumpSpeed, walkJump, Time.deltaTime);
                }
            }
        }

        private void RunPressed() {_moveState = MoveState.Run;}
        private void RunReleased() {_moveState = MoveState.Walk;}

        private void RotateNavigator(Vector3 ? finalFacing = null)
        {
            if (_slideState == SlideState.None)
            {
                if (finalFacing == null)
                {
                    if (_charController.isGrounded)
                    {
                        if (headingRotated != Vector3.zero)
                        {
                            _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), turnLerp);
                        }
                    }
                    else
                    {
                        if (velocity != Vector3.zero)
                        {
                            _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(velocity.normalized), turnLerp);
                        }
                    }
                }
                else
                {
                    if (finalFacing.Value != null) _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(finalFacing.Value), turnLerp);
                    Debug.Log("Final Facing: " + finalFacing.Value);
                }
            }
            else { if (headingRotated!= Vector3.zero) _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), turnLerp); }
        }

        private void SlideCheck()
        {
            if (_slideState == SlideState.None)
            {
                if (Vector3.Angle(velocity, headingRotated) >= 90) { _slideState = SlideState.Slide;}
            }
            else if (_slideState == SlideState.Slide)
            {
                if (Mathf.Sign(velocity.x) == Mathf.Sign(headingRotated.x) && Mathf.Sign(velocity.z) == Mathf.Sign(headingRotated.z) || velocity.magnitude < walkMax)
                {
                    _slideState = SlideState.None; if(_jumpQueued) {_jumpState = JumpState.NormalJump; Jump(); _jumpQueued = false; Debug.Log("Slide Jump");}
                }
            }
        }

    }
}