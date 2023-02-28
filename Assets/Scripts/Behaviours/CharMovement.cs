using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController
{
    public class CharMovement : MonoBehaviour
    {

        [Header("Objects & Components")] // Variables containing objects and components
        private Camera _camera;
        private Rigidbody _playerRb;
        private CharacterController _charController;
        private Transform _navigator;

        [Header("Movement Variables")] // Variables governing character movement and orientation
        public float walkMax;
        public float runMax;
        public Vector3 momentum;
        public Vector3 momentumCheck;
        public float inertia;
        public float ySpeed;
        private float originalStepOffset;
        public float magnitude;
        private Vector2 moveInput;
        private Vector3 charDirection;
        public Vector3 velocity;
        public Vector3 priorVelocity;
        public Vector3 heading;
        public Vector3 headingRotated;
        public Vector3 _currentMovement;
        public float charTurnLerp;
        public float acceleration;
        public float groundDrag;
        public float currentSpeed;
        public float wallTracker = 0f;
        public float speedDial;

        [Header("Jump Variables")] // Variables governing jump motion.
        public float jumpSpeed;

        [Header("Jump Queueing")]
        private const float _jumpBuffer = .15f; // How long prior to landing can the jump input be triggered?
        private float _jumpTracker; // The time the last mid-air jump input was pressed.
        private bool _jumpQueued; // Is a jump currently queued for landing?

        [Header("Coyote Time")]
        private const float _coyoteTime = .12f; // The player can still jump as if they are on the edge.
        private float _coyoteTracker; // The last time the player became airborne (without jumping)
        private bool _coyoteAvailable = true;

        [Header("Movement States")] // Variables governing character states
        public bool playerRunning;
        public bool playerGrounded;
        public bool heelTurn;

        void Awake()
        {
            momentum = new Vector3(0,0,0);
            InitializeMovement();
        }

        // Update is called once per frame
        void Update()
        {
            //DetectWalls();
            if (_charController.isGrounded) // Executed if player's character controller is grounded.
            {
                ySpeed = -0.5f; // ySpeed must be reset every frame the character is on the ground in order to allow them to move under simulated gravity.

                if (!playerGrounded) // If boolean playerGrounded is set to False
                {
                    // Use the BecomeGrounded method
                    BecomeGrounded();
                }

                InputToHeading(); // Converts player input into a heading for the player character.
                PlayerRun(); // Checks whether the player is running 
                Jump();  
            } 
            else // Executed if player's character controller is airborne.
            {
                if (playerGrounded)
                {
                    BecomeAirborne(); // Use the BecomeAirborne method.
                }

                if (heading.x == 0 && heading.z == 0) // The following if statement checks whether the player is moving horizontally at time of jump
                {
                    bool clampInput = true;
                    InputToHeading(clampInput);
                }

                AirborneBehaviour();
                
            }
            //SpeedHandler();
            ySpeed += Physics.gravity.y * Time.deltaTime;

            NewMoveTest();
            //MoveChar();
            
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
            DetectHeadbumps();
            //DetectWalls();
            if (_coyoteAvailable && (_coyoteTracker + _coyoteTime >= Time.time)) // If Coyote Time is available, and the time since the player left ground + the grace period is less than the current time
            {
                    Jump();  
            } else if (!_jumpQueued)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    _jumpQueued = true;
                    _jumpTracker = Time.time;
                }
            }
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
                ySpeed += jumpSpeed;
                _jumpQueued = false;
            } else {
                _charController.stepOffset = originalStepOffset;
                playerGrounded = true;
                _coyoteAvailable = true;
                _jumpQueued = false;
            }
        }

        // Ensures if you hit your head on something while jumping you don't hang under it until gravity takes effect.
        private void DetectHeadbumps()
        {
            if ((_charController.collisionFlags & CollisionFlags.Above) != 0 && (velocity.y > 0)) 
            {
                Debug.Log("Headbump");
                ySpeed -=(velocity.y * 4);
            }
        }

        // Prevents the player from accumulating velocity and momentum while running against a wall.
        /* private void DetectWalls()
        {
            float speedLimit = playerRunning ? runMax : walkMax;
            if ((_charController.collisionFlags & CollisionFlags.Sides) != 0)
            {
                if (currentSpeed > (speedLimit / 2))
                {
                    currentSpeed = currentSpeed / 2;
                }
                else
                {
                    currentSpeed -= acceleration * 2 * Time.deltaTime;  
                }
               
            }

        } */
        
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
            moveInput = new Vector2(Input.GetAxis("HorizontalKey"), Input.GetAxis("VerticalKey")); // Gets movement input
            if (clampInput)
            {
                // If clampInput is true, the player's input only counts for 1/6 of its usual value
                // This is used if the player jumps from a standing position, allowing them just enough movement to get onto nearby ledges, etc.
                heading = new Vector3(moveInput.x, 0.0f, moveInput.y);
                //currentSpeed = 1;
            } 
            else
            {
                heading = new Vector3(moveInput.x, 0.0f, moveInput.y);
            }
            headingRotated = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * heading;            
            headingRotated.Normalize();
        }

        public void NewMoveTest()
        {
            float speedLimit = playerRunning ? runMax : walkMax;
            if (headingRotated.x != 0f)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, speedLimit * headingRotated.x, acceleration * Time.deltaTime);
            }
            else
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0f, groundDrag * Time.deltaTime);
            }
            if (headingRotated.z != 0f)
            {
                velocity.z = Mathf.MoveTowards(velocity.z, speedLimit * headingRotated.z, acceleration * Time.deltaTime);
            }
            else
            {
                velocity.z = Mathf.MoveTowards(velocity.z, 0f, groundDrag * Time.deltaTime);
            }
            
            Vector3 move = velocity * speedDial * Time.deltaTime;

            Debug.Log("Velocity: " + velocity + ", Move: " + move);

            move = AdjustVelocityToSlope(move);
            move.y += ySpeed;

            _charController.Move(move * Time.deltaTime);
            
            if (headingRotated != Vector3.zero)
            {
                _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), charTurnLerp);

            }

        }

        private void Jump()
        {
            if (Input.GetButtonDown("Jump"))
            {
                playerGrounded = false;
                _coyoteAvailable = false;   
                ySpeed += jumpSpeed;
            }
        }

        /* private void MoveChar()
        {
            velocity = headingRotated * currentSpeed;
            Vector3 move = momentum + velocity;
            float speedLimit = playerRunning ? runMax : walkMax;
            move = move.normalized * speedLimit;
            Debug.Log("Momentum: " + momentum + ", Velocity: " + velocity + ", Move: " + move);
            InertiaHandler(move);
            //if (velocity != Vector3.zero) { momentum = velocity; }
            move = AdjustVelocityToSlope(move);
            move.y += ySpeed;


            _charController.Move(move * Time.deltaTime);
        

            if (headingRotated != Vector3.zero)
            {
                _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), 0.2f);

            }
        } */

        private void PlayerRun()
        {
            // Changes player's running state based on Left Shift
            if(Input.GetAxis("RunToggle") == 1)
            {
                playerRunning = true;
                charTurnLerp = 0.2f;
            } else {
                playerRunning = false;
                charTurnLerp = 0.06f;
            }
        }

        /* private void SpeedHandler()
        {
            float angleDelta = Vector3.Angle(headingRotated, momentum.normalized);
            if (moveInput != Vector2.zero)
            {
                float speedLimit = playerRunning ? runMax : walkMax;
                if (currentSpeed <= speedLimit) {currentSpeed += acceleration * Time.deltaTime;}
                else {currentSpeed -= groundDrag * Time.deltaTime;}

                if (angleDelta >= 90 &&  angleDelta <= 135)
                {
                    currentSpeed -= (currentSpeed / 2);
                }
                else if (angleDelta >= 135) 
                {
                    currentSpeed = 0.0f;
                    heelTurn = true;
                    //Debug.Log("bunt");
                }
            }
            else
            {
                if (currentSpeed > 0)
                {
                    currentSpeed -= groundDrag * Time.deltaTime;
                    if (currentSpeed < 0) { currentSpeed = 0; }
                }
            }
            ApplyFriction(); 
        } */

        /* protected void ApplyFriction()
        {
            float xMomentumMultiplier = Mathf.Sign(momentum.x) == 1 ? -1 : 1;
            float zMomentumMultiplier = Mathf.Sign(momentum.z) == 1 ? -1 : 1;
            float dragMultiplier = heelTurn ? groundDrag * 2 : groundDrag;
            float xTemp;
            float zTemp;  

            if (xMomentumMultiplier == -1) xTemp = Mathf.Clamp(momentum.x + (dragMultiplier * xMomentumMultiplier * Time.deltaTime), 0, momentum.x);
            else xTemp = Mathf.Clamp(momentum.x + (dragMultiplier * xMomentumMultiplier * Time.deltaTime), momentum.x, 0);

            if (zMomentumMultiplier == -1) zTemp = Mathf.Clamp(momentum.z + (dragMultiplier * zMomentumMultiplier * Time.deltaTime), 0, momentum.z);
            else zTemp = Mathf.Clamp(momentum.z + (dragMultiplier * zMomentumMultiplier * Time.deltaTime), momentum.z, 0);
            momentum = Vector3.MoveTowards(momentum, new Vector3(xTemp, 0.0f, zTemp), dragMultiplier * Time.deltaTime);

            if (momentum.x == 0 && momentum.z == 0) heelTurn = false;
        } */

        /* protected Vector3 InertiaHandler(Vector3 move)
        {
            //float speedLimit = playerRunning ? runMax : walkMax;
            if ((Mathf.Sign(velocity.x) == Mathf.Sign(momentum.x) || momentum.x == 0.0f) && velocity.x != 0.0f)
            {
                //Debug.Log("Velocity X: " + Mathf.Sign(velocity.x) + ", Momentum X: " + Mathf.Sign(momentum.x));
                momentum.x = velocity.x;
            } else {}
            
            if ((Mathf.Sign(velocity.z) == Mathf.Sign(momentum.z) || momentum.z == 0.0f) && velocity.z != 0.0f)
            {
                momentum.z = velocity.z;
            }
            return move;
        } */

    }
}