using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController
{
    public class CharController : MonoBehaviour
    {

        private Camera _camera;

        // Sets character's movespeed.
        public float walkSpeed;
        public float maxSpeed;
        public float jumpSpeed;
        public float jumpForce;
        public float momentum;
        private Vector2 moveInput;
        private Vector3 charDirection;
        private Vector3 velocity;
        private float ySpeed;
        private float originalStepOffset;
        private Vector3 heading;

        // Sets variable for character's rigidbody
        private Rigidbody _playerRb;
        private CharacterController _charController;
        private Transform _navigator;


        // Character States
        public bool canJump;
        public bool isRunning;
        public float runTime = 2.0f;

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
            if (_charController.isGrounded)
            {
                ResetCoyote();
                moveInput = new Vector2(Input.GetAxis("HorizontalKey"), Input.GetAxis("VerticalKey")).normalized;
                heading = new Vector3(moveInput.x, 0.0f, moveInput.y);
            } 
            else 
            {
                if(canJump)
                {
                    StartCoroutine(CoyoteTime());
                }
                DetectHeadbumps();
            }

            Vector3 headingRotated = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * heading;
            float magnitude;
            // Crude Run functionality below, replace later. Probably by having a single speed variable updated in the run method.
            if (isRunning == true)
            {
                magnitude = Mathf.Clamp01(headingRotated.magnitude) * maxSpeed;
            }
            else
            {
                magnitude = Mathf.Clamp01(headingRotated.magnitude) * walkSpeed; 
            }
            headingRotated.Normalize();

            ySpeed += Physics.gravity.y * Time.deltaTime;

            if (_charController.isGrounded == true)
            {
                _charController.stepOffset = originalStepOffset;
                ySpeed = -0.5f;
                playerRun();

                if (Input.GetButtonDown("Jump"))
                {
                    ySpeed = jumpSpeed;
                    canJump = false;
                }

            }
            else
            {
                _charController.stepOffset = 0;
            }

            velocity = headingRotated * magnitude;
            velocity = AdjustVelocityToSlope(velocity);
            velocity.y += ySpeed;


                _charController.Move(velocity * Time.deltaTime);
        

            if (headingRotated != Vector3.zero)
            {
                _navigator.rotation = Quaternion.Slerp(_navigator.rotation, Quaternion.LookRotation(headingRotated.normalized), 0.2f);

            }
            
            
        }

        // Resets Coyote Time. Called when player is on the ground.
        private void ResetCoyote()
        {
        
            canJump = true;
    
        }

        // Count x seconds before disabling ability to jump. To be called when jumping, or when the player is not colliding with anything below them.
        private IEnumerator CoyoteTime()
        {
            float duration = 1f;
            // Insert code here that will disable canJump after duration.
            yield return null;
        }

        // Ensures if you hit your head on something while jumping you don't hang under it until gravity takes effect.
        private void DetectHeadbumps()
        {
            if ((_charController.collisionFlags & CollisionFlags.Above) != 0 && (velocity.y > 0)) 
            {
                Debug.Log("Headbump");
                ySpeed += -5;
            }
        }
        
        private void playerRun()
        {
            // Changes player's running state based on Left Shift, but only if they are on the ground
            if(Input.GetAxis("RunToggle") == 1)
            {
                isRunning = true;
            } else {
                isRunning = false;
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
    }
}