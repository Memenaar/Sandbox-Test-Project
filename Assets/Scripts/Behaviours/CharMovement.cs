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
        public float walkSpeed;
        public float maxSpeed;
        public float momentum;
        public float ySpeed;
        private float originalStepOffset;
        public float magnitude;
        private Vector2 moveInput;
        private Vector3 charDirection;
        public Vector3 velocity;
        public Vector3 heading;
        public Vector3 headingRotated;

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

        void Awake()
        {
            InitializeMovement();
        }

        // Update is called once per frame
        void Update()
        {
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

            headingRotated = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * heading;
            
            // Crude Run functionality below, replace later. Probably by having a single speed variable updated in the run method.
            if (playerRunning == true)
            {
                magnitude = Mathf.Clamp01(headingRotated.magnitude) * maxSpeed;
            }
            else
            {
                magnitude = Mathf.Clamp01(headingRotated.magnitude) * walkSpeed; 
            }
            headingRotated.Normalize();

            ySpeed += Physics.gravity.y * Time.deltaTime;

            MoveChar();
            
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
                ySpeed += -velocity.y;
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
            moveInput = new Vector2(Input.GetAxis("HorizontalKey"), Input.GetAxis("VerticalKey")).normalized; // Gets movement input
            if (clampInput)
            {
                // If clampInput is true, the player's input only counts for 1/6 of its usual value
                // This is used if the player jumps from a standing position, allowing them just enough movement to get onto nearby ledges, etc.
                heading = new Vector3(moveInput.x/6, 0.0f, moveInput.y/6);
            } 
            else
            {
                heading = new Vector3(moveInput.x, 0.0f, moveInput.y);
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

        private void MoveChar()
        {
            velocity = headingRotated * magnitude;
            velocity = AdjustVelocityToSlope(velocity);
            velocity.y += ySpeed;


                _charController.Move(velocity * Time.deltaTime);
        

            if (headingRotated != Vector3.zero)
            {
                _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), 0.2f);

            }
        }

        private void PlayerRun()
        {
            // Changes player's running state based on Left Shift
            if(Input.GetAxis("RunToggle") == 1)
            {
                playerRunning = true;
            } else {
                playerRunning = false;
            }
        }
    }
}