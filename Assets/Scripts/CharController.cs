using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController
{
    public class CharController : MonoBehaviour
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
        private const float _jumpBuffer = 20f; // How long prior to landing can the jump input be triggered?
        public float _jumpPressedMidAir_Buffer; // The time the last mid-air jump input was pressed.
        public bool jumpQueued; // Is a jump currently queued for landing?

        [Header("Coyote Time")]
        private const float _coyoteTime = 10f; // The player can still jump as if they are on the edge.
        private float _coyoteTracker; // The last time the player became airborne (without jumping)
        public bool _coyoteAvailable = true;

        [Header("States")] // Variables governing character states
        public bool playerRunning;
        public bool playerGrounded;

        void Awake()
        {
            _camera = Camera.main;
            _playerRb = GetComponent<Rigidbody>();
            _navigator = gameObject.transform.Find("Navigator").GetComponent<Transform>();
            _charController = GetComponent<CharacterController>();
            originalStepOffset = _charController.stepOffset;
        }

        // Update is called once per frame
        void Update()
        {
            // if player's character controller is grounded
            if (_charController.isGrounded)
            {
                // and boolean playerGrounded is set to False
                if (!playerGrounded)
                {
                    // Use the BecomeGrounded method
                    BecomeGrounded();

                } else {
                moveInput = new Vector2(Input.GetAxis("HorizontalKey"), Input.GetAxis("VerticalKey")).normalized;
                heading = new Vector3(moveInput.x, 0.0f, moveInput.y);
                }
                
                _charController.stepOffset = originalStepOffset;
                ySpeed = -0.5f;
                playerRun();

                if (Input.GetButtonDown("Jump"))
                {
                    Jump();  
                }
            } 
            else // if player's character controller is NOT grounded
            {
                if (_coyoteAvailable && (_coyoteTracker + _coyoteTime >= Time.time)) // If Coyote Time is available, and the time since the player left ground + the grace period is less than the current time
                {
                    if (Input.GetButtonDown("Jump"))
                    {
                        Jump();  
                    }
                } else if (playerGrounded)
                {
                    BecomeAirborne(); // Use the BecomeAirborne method.
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

        // Ensures if you hit your head on something while jumping you don't hang under it until gravity takes effect.
        private void DetectHeadbumps()
        {
            if ((_charController.collisionFlags & CollisionFlags.Above) != 0 && (velocity.y > 0)) 
            {
                Debug.Log("Headbump");
                ySpeed += -velocity.y;
            }
        }
        
        private void playerRun()
        {
            // Changes player's running state based on Left Shift, but only if they are on the ground
            if(Input.GetAxis("RunToggle") == 1)
            {
                playerRunning = true;
            } else {
                playerRunning = false;
            }
        }

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

        private void AirborneBehaviour()
        {
            DetectHeadbumps();
            if (!jumpQueued && Input.GetButtonDown("Jump"))
            {
                Debug.Log("Jump queued");
                jumpQueued = true;
                _jumpPressedMidAir_Buffer = Time.time;
            }
        }

        private void BecomeGrounded()
        {
            // Check whether a jump is buffered.
            if (jumpQueued && (_jumpPressedMidAir_Buffer + _jumpBuffer > Time.time))
            {
                Debug.Log("Queue Check Succeeded");
                Jump();
            } else {
                playerGrounded = true;
                _coyoteAvailable = true;
                jumpQueued = false;
            }
        }

        private void BecomeAirborne()
        {
            playerGrounded = false;
            _coyoteTracker = Time.time; // Set the last "leftGround" time to the current time.
            _charController.stepOffset = 0; // Set the character controller's step offset to 0. Prevents characters mantling mid-air.
        }

        private void Jump()
        {
            playerGrounded = false;
            _coyoteAvailable = false;   
            if(!jumpQueued){ // If the jump is NOT a queued jump
                ySpeed += jumpSpeed;
            } 
            else // If the jump IS a queued jump
            {
                ySpeed = -0.5f + jumpSpeed;
                jumpQueued = false;
                Debug.Log("Queued Jump Executed");
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
    }
}