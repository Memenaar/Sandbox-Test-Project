/*using System.Collections;
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
        public CharacterController _charController;
        private Transform _navigator;
        private GameInput _gameInput;


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
            _gameInput = new GameInput();
            _gameInput.TownState.RunStart.performed += x => RunPressed();
            _gameInput.TownState.RunFinish.performed += x => RunReleased();
            _gameInput.TownState.Jump.performed += x => JumpLogic();
        }
        
        void OnEnable()
        {
            _gameInput.TownState.Enable();
        }
        
        void OnDisable()
        {
            _gameInput.TownState.Disable();
        }

        // Update is called once per frame
        void Update()
        {
            if (_gameInput.TownState.Jump.WasPressedThisFrame())
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
                moveInput = _gameInput.TownState.Movement.ReadValue<Vector2>(); // Gets movement input

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
                    if (playerGrounded) BecomeAirborne(); // Use the BecomeAirborne method if player is just entering air.
                    AirborneBehaviour(); // Execute airborne behavior method
                    if (_jumpState == JumpState.StandingJump && _moveState != MoveState.Locked) InputToHeading(true); // if Jump State is Standing Jump and move State is NOT locked, input to heading with InputClamped = true.
                
                }  
            }
            
            MoveChar(); // Move Character method
            
        }

        // Called when the character controller detects a collision
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _wallNormal = new Vector3(hit.normal.x, 0.0f, hit.normal.z); // Get the x and z axes of the collision's normal. Omission of y axis prevents undue up/downward momentum in wall jump.

            if (_charController.isGrounded) // If Char is grounded 
            {
                if (_charController.collisionFlags == CollisionFlags.Below) // If character is ONLY touching ground
                {
                    // No further action
                }
                else if ((_charController.collisionFlags & CollisionFlags.Sides) != 0) // If character is touching sides
                {
                    // Clamps player velocity in direction of impact, but only if they are fully moving in that direction.
                    if (Mathf.Abs(_wallNormal.x) == 1) velocity.x = Mathf.Clamp(velocity.x, -1, 1);
                    if (Mathf.Abs(_wallNormal.z) == 1) velocity.z = Mathf.Clamp(velocity.z, -1, 1);
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

                    // If the player is moving faster than their walk speed, their jump is still in its upward stage, the surface they collide with is tagged for wall jumping, and their jump state is NOT standing jump or none...
                    if (velocity.magnitude > walkMax && ySpeed > 0 && hit.gameObject.CompareTag("WallJump") && _jumpState != (JumpState.StandingJump | JumpState.None))
                    {
                        ySpeed = 0; // Set player's ySpeed to 0
                        _jumpState = JumpState.None; // Reset the Jump State
                        _slideState = SlideState.WallSlide; // Enter the wall slide state
                    }
                    else if (_jumpState != JumpState.None) // If the above conditions are not met, and the Jump State is NOT none
                    {
                        _jumpState = JumpState.StandingJump; // Set the jump state to Standing Jump so the player isn't stuck with their velocity driving them into the wall
                    }
                }
            }
        }

        // METHODS BELOW THIS POINT
        

        // Fixes slope bounce
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
                _jumpQueued = false; // Reset Jump Queue
            } else {
                _charController.stepOffset = originalStepOffset; // Return original Step Offset value so stairs can be used.
                playerGrounded = true; // Mark the player as grounded if they are just touching down
                _coyoteAvailable = true; // Reset Coyote Time.
                _jumpQueued = false; // Reset Jump Queue
                _jumpState = JumpState.None; // Reset Jump State
            }
        }

        private void HeadBump() // Ensures if you hit your head on something while jumping you don't hang under it until gravity takes effect.
        {
                velocity.x += velocity.x * -5 * Time.deltaTime; // Reduce x velocity
                velocity.z += velocity.z * -5 * Time.deltaTime; // Reduce z velocity
                ySpeed += ySpeed * -1.5f; // Multiply ySpeed by 1.5 and invert.
        }
        
        protected void InitializeMovement() // Initialize needed game objects, components, and variables.
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
                heading = new Vector3(moveInput.x, 0.0f, moveInput.y); // Create Heading vector from moveinput.
                headingRotated = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * heading; // Rotate heading relative to camera direction       
                headingRotated.Normalize(); // Normalize the rotated heading.
                if (clampInput) {headingRotated = headingRotated * 0.25f;} // Reduces effect of input to 1/4 is clampInput is true.

        }

        private void Jump(Vector3 ? horizontalPower = null)
        {

            float jumpForce = _jumpState == JumpState.WallJump ? (jumpSpeed * 0.75f) : jumpSpeed;
            float speedLimit = _moveState == MoveState.Run ? runMax : walkMax;
            velocity = Vector3.ClampMagnitude((headingRotated * velocity.magnitude), runJump);
            if (horizontalPower != null) // If an override is provided
            {
                velocity = Vector3.ClampMagnitude((horizontalPower.Value * speedLimit), runJump); // Set velocity = the input horizontal power value, multiplied by the current SpeedLimit, with a max value = the character's running jump speed.
            }
            _coyoteAvailable = false; // Mark Coyote Time as unavailable.
            ySpeed += jumpForce; // Add the current JumpForce to the player's vertical speed.
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
                    if (_slideState == SlideState.WallSlide) {_jumpState = JumpState.WallJump; _slideState = SlideState.None; Jump(_wallNormal); Debug.Log(_wallNormal);} // Wall Jump
                    else 
                    {
                        if (_coyoteAvailable && (_coyoteTracker + _coyoteTime >= Time.time)) { _jumpState = JumpState.NormalJump; Jump();} // Coyote Time jump                    
                        else if (!_jumpQueued) { _jumpQueued = true; _jumpTracker = Time.time;} // Queues a jump for landing
                    }
                }
            }
            else if (!_coyoteAvailable && !_jumpQueued) { _jumpQueued = true; _jumpTracker = Time.time;} // Queues a jump for landing
            
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
                    }
                    else
                    {
                        if (velocity != Vector3.zero)
                        {
                            _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(velocity.normalized), turnLerp);
                        }
                    }
            }
            else { if (headingRotated!= Vector3.zero) _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), turnLerp); }
        }

        private void RunPressed() {_moveState = MoveState.Run;}
        private void RunReleased() {_moveState = MoveState.Walk;}

        private void SlideCheck()
        {
            if (_slideState == SlideState.None)
            {
                if (Vector3.Angle(velocity, headingRotated) >= 90 && velocity.magnitude >= walkMax) { _slideState = SlideState.Slide;}
            }
            else if (_slideState == SlideState.Slide)
            {
                if (Mathf.Sign(velocity.x) == Mathf.Sign(headingRotated.x) && Mathf.Sign(velocity.z) == Mathf.Sign(headingRotated.z))
                {
                    _slideState = SlideState.None; if(_jumpQueued) {_jumpState = JumpState.NormalJump; Jump(); _jumpQueued = false; Debug.Log("Slide Jump");}
                }
            }
        }

    }
} */