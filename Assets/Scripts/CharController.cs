using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController
{
    public class CharController : MonoBehaviour
    {

        private Camera _camera;
        Vector3 camF;   
        Vector3 camR;

        // Sets character's movespeed.
        public float walkSpeed;
        public float maxSpeed;
        public float jumpSpeed;
        public float jumpForce;
        public float momentum;
        private Vector2 moveInput;
        private Vector3 charDirection;
        private float ySpeed;
        private float tempStepOffset;

        // Sets variable for character's rigidbody
        private Rigidbody _playerRb;
        private CharacterController _charController;
        private Transform _navigator;

        // Creates a transform named _t
        private Transform _t;

        // Character States
        public bool isOnGround;
        public bool isRunning;
        public float runTime = 2.0f;

        void Awake()
        {
            _camera = Camera.main;
            _playerRb = GetComponent<Rigidbody>();
            _navigator = gameObject.transform.Find("Navigator").GetComponent<Transform>();
            _charController = GetComponent<CharacterController>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _t = transform;
            camF = _camera.transform.forward;   
            camR = _camera.transform.right;
            isOnGround = true;
        }

        // Update is called once per frame
        void Update()
        {
            moveInput = new Vector2(Input.GetAxis("HorizontalKey"), Input.GetAxis("VerticalKey")).normalized;

            CameraUpdate();
            if (isOnGround == true)
            {
                ySpeed = -0.5f;
                playerRun();
            }
            if ((Input.GetAxis("HorizontalKey") != 0 || Input.GetAxis("VerticalKey") != 0))
            {
                charDirection = Move();
            }

            Jump();
    
            
        }
        private void CameraUpdate()
        {
            camF = _camera.transform.forward;   
            camR = _camera.transform.right;
            camF = new Vector3(camF.x, 0, camF.z).normalized;
            camR = new Vector3(camR.x, 0, camR.z).normalized;
        }

        private Vector3 Move()
        {
            Vector3 heading = new Vector3(moveInput.x, 0.0f, moveInput.y);
            //float magnitude = Mathf.Clamp01(heading.magnitude) * walkSpeed;
            heading.Normalize();
            Vector3 headingRotated = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * heading;    

            //ySpeed += Physics.gravity.y * Time.deltaTime;   
            //Vector3 velocity = heading * magnitude;
            //velocity.y = ySpeed;

            if (isOnGround == true && heading != Vector3.zero)
            {
                if (isRunning)
                {
                    _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), 0.2f);
                    //_playerRb.MovePosition(velocity * Time.deltaTime);
                    transform.position += (camF*moveInput.y + camR*moveInput.x) * Time.deltaTime * maxSpeed;
                } else {
                    _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), 0.2f);
                    //_playerRb.MovePosition(velocity * Time.deltaTime);
                    transform.position += (camF*moveInput.y + camR*moveInput.x) * Time.deltaTime * walkSpeed;
                }
            }
            return headingRotated;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.CompareTag("Ground"))
            {
                isOnGround = true;
            }
        }

        private void Jump()
        {
            if (isOnGround == true && Input.GetKeyDown(KeyCode.Space))
            {
                // This code needs replacing.
                // Goals for new jump should be:
                // 1. Only usable on ground
                // 2. Jumps straight up if char is not moving
                // 3. if char IS moving, maintains horizontal momentum during jump
                // 4. Locked out of new horizontal movement in the air (this should already be covered by the move function)
                Vector3 jumpVector = (charDirection + Vector3.up);
                _playerRb.AddForce(jumpVector * jumpForce, ForceMode.Impulse);
                isOnGround = false;
            }
        }
        
        private void playerRun()
        {
            // Changes player's running state based on Left, but only if they are on the ground
            if(Input.GetAxis("RunToggle") == 1)
            {
                isRunning = true;
            } else {
                isRunning = false;
            }
        }

        // This is meant to fix our slope bounce.
        // Currently commented lines are related to this solution as shown here: https://www.youtube.com/watch?v=PEHtceu7FBw&ab_channel=KetraGames
        // May want to consider changing to a Character Controller, or copying some of the character controller's functionality.
        // More info here: https://medium.com/ironequal/unity-character-controller-vs-rigidbody-a1e243591483
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
    }
}