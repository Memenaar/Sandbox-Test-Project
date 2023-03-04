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
        public bool playerRunning;
        public bool playerGrounded;
        public bool moveLocked;
        public bool playerSliding;
        public bool canWallJump;
        public bool airWiggle;
        public bool wallJumped;

        void Awake()
        {
            InitializeMovement();

            // Value Initialization
            groundDrag = walkDrag;
            acceleration = walkAccel;
            turnLerp = walkLerp;
            moveLocked = false;


            // Player Action initialization
            _playerActions = new PlayerActions();
            _playerActions.WorldGameplay.RunStart.performed += x => RunPressed();
            _playerActions.WorldGameplay.RunFinish.performed += x => RunReleased();
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
            if (!moveLocked) {
                moveInput = _playerActions.WorldGameplay.Movement.ReadValue<Vector2>();
            } // Gets movement input
            DetectWalls();
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
                JumpLogic();  
            } 
            else // Executed if player's character controller is airborne.
            {
                AirborneBehaviour();

                if ((heading.x == 0 && heading.z == 0) && !wallJumped) // The following if statement checks whether the player is moving horizontally at time of jump
                {
                    InputToHeading(true);
                }
                //DetectHeadbumps();
               
            }

            ySpeed += Physics.gravity.y * Time.deltaTime;
            MoveChar();
            
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!_charController.isGrounded)
            {
                _wallNormal = hit.normal;
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
            if (playerGrounded)
            {
                BecomeAirborne(); // Use the BecomeAirborne method.
            }
            if(airWiggle) InputToHeading(true);
            JumpLogic();
        }

        private void BecomeAirborne()
        {
            playerGrounded = false;
            _coyoteTracker = Time.time; // Set the last "leftGround" time to the current time.
            _charController.stepOffset = 0; // Set the character controller's step offset to 0. Prevents characters mantling mid-air.
        }

        private void BecomeGrounded()
        {
            // Check whether a jump is buffered.
            if (_jumpQueued && (_jumpTracker + _jumpBuffer > Time.time))
            {
                Jump();
                _jumpQueued = false;
            } else {
                _charController.stepOffset = originalStepOffset;
                playerGrounded = true;
                _coyoteAvailable = true;
                _jumpQueued = false;
                moveLocked = false;
                airWiggle = false;
                wallJumped = false;
            }
        }

        // Ensures if you hit your head on something while jumping you don't hang under it until gravity takes effect.
        private void DetectHeadbumps()
        {
            if ((_charController.collisionFlags == CollisionFlags.Above)) 
            {
                Debug.Log("Headbump");
                velocity.x = 0.0f;
                velocity.z = 0.0f;
                ySpeed -= ySpeed * 1.5f;
                moveLocked = true;
            }
        }

        // Prevents the player from accumulating velocity and momentum while running against a wall.
        private void DetectWalls()
        {
            float speedLimit = playerRunning ? runMax : walkMax;
            if ((_charController.collisionFlags & CollisionFlags.Sides) != 0)
            {
                if (_charController.isGrounded) {velocity = Vector3.ClampMagnitude(velocity, walkMax);}
                else if (!_charController.isGrounded & (ySpeed > 0))
                {
                    canWallJump = true;
                    JumpLogic();
                }
            }
            else
            {
                canWallJump = false;
            }

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
                if (clampInput) {headingRotated = headingRotated * 0.25f;Debug.Log("Fuck off"); }; // Reduces input's effect to 1/4 is clampInput is true.

        }

        private void JumpLogic()
        { 
            if (_playerActions.WorldGameplay.Jump.triggered)
            {
                if(_charController.isGrounded)
                {
                    if (playerSliding && !_jumpQueued) {_jumpQueued = true;}
                    else {Jump();}
                }
                else
                {
                    if (!_coyoteAvailable && !_jumpQueued) { _jumpQueued = true; _jumpTracker = Time.time;}
                    else if (_coyoteAvailable && (_coyoteTracker + _coyoteTime >= Time.time)) { Jump();}
                    else if (canWallJump && ((Mathf.Abs(velocity.x) > walkMax) || (Mathf.Abs(velocity.z) > walkMax))) {wallJumped = true; Jump(_wallNormal);}
                }

            }
        }

        private void Jump(Vector3 ? horizontalPower = null)
        {
            float jumpForce = wallJumped ? (jumpSpeed * 0.5f) : jumpSpeed;
            float speedLimit = playerRunning ? runMax : walkMax;
            if (horizontalPower != null)
            {
                playerGrounded = false; _coyoteAvailable = false; ySpeed += jumpForce; velocity = Vector3.ClampMagnitude((horizontalPower.Value * velocity.magnitude), runJump); Debug.Log(velocity);
                RotateNavigator(velocity);
                Debug.Log("Crap");
            }
            else
            {
                playerGrounded = false; _coyoteAvailable = false; ySpeed += jumpForce;
                if (headingRotated == Vector3.zero) airWiggle = true;
                if (velocity != Vector3.zero) 
                {
                    velocity = Vector3.ClampMagnitude(velocity = Vector3.zero + (headingRotated * velocity.magnitude * jumpForce), speedLimit);
                    RotateNavigator(headingRotated.normalized);
                }
                Debug.Log("Piss");
            }
        }



        private void MoveChar()
        {
            if (!moveLocked)
            {
            float speedLimit = playerRunning ? runMax : walkMax;
            float dragFactor = playerRunning ? runDrag : walkDrag;
            float accelFactor = playerRunning ? runAccel : walkAccel;
            if (headingRotated != Vector3.zero)
            {
                if(_charController.isGrounded)
                {
                    velocity = Vector3.MoveTowards(velocity, new Vector3(speedLimit * headingRotated.x, 0.0f, speedLimit * headingRotated.z), acceleration * Time.deltaTime);
                }
                else if(!_charController.isGrounded && airWiggle)
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
            if(playerRunning == true)
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

        private void RunPressed() {playerRunning = true;}
        private void RunReleased() {playerRunning = false;}

        private void RotateNavigator(Vector3 ? finalFacing = null)
        {
            if (!playerSliding)
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
                    Debug.Log("Oink " + finalFacing.Value);
                }
            }
            else { if (headingRotated!= Vector3.zero) _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), turnLerp); }
        }

        private void SlideCheck()
        {
            if (!playerSliding)
            {
                if (Vector3.Angle(velocity, headingRotated) >= 90) { playerSliding = true;}
            }
            else
            {
                if (Mathf.Sign(velocity.x) == Mathf.Sign(headingRotated.x) && Mathf.Sign(velocity.z) == Mathf.Sign(headingRotated.z))
                {
                    playerSliding = false; if(_jumpQueued) {Jump((headingRotated.normalized * jumpSpeed)); _jumpQueued = false; Debug.Log("lmao");}
                }
            }
        }

    }
}