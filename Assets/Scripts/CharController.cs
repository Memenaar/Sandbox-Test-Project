using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController
{
    public class CharController : MonoBehaviour
    {

        private Camera _camera;

        // Sets character's movespeed.
        public float moveSpeed;
        public float maxSpeed;
        public float jumpSpeed;
        public float jumpForce;
        public float momentum;
        private Vector2 moveInput;
        private Vector3 charDirection;

        // Sets variable for character's rigidbody
        private Rigidbody playerRb;

        // Creates a transform named _t
        private Transform _t;

        // Creates a Vector2 named charRotation
        public Vector2 charRotation;

        // Character States
        public bool isOnGround;

        void Awake()
        {
            _camera = Camera.main;
            playerRb = GetComponent<Rigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _t = transform;
            Vector3 camF = _camera.transform.forward;   
            Vector3 camR = _camera.transform.right;
            isOnGround = true;
        }

        // Update is called once per frame
        void Update()
        {
            if ((Input.GetAxis("HorizontalKey") != 0 || Input.GetAxis("VerticalKey") != 0))
            {
                charDirection = Move();
            }

            if (isOnGround == true && Input.GetKeyDown(KeyCode.Space))
            {
                // This code needs replacing.
                // Goals for new jump should be:
                // 1. Only usable on ground
                // 2. Jumps straight up if char is not moving
                // 3. if char IS moving, maintains horizontal momentum during jump
                // 4. Locked out of new horizontal movement in the air (this should already be covered by the move function)
                Vector3 jumpVector = (charDirection + Vector3.up);
                playerRb.AddForce(jumpVector * jumpForce, ForceMode.Impulse);
                isOnGround = false;
            }
    
            
        }

        private Vector3 Move()
        {
            Vector3 camF = _camera.transform.forward;   
            Vector3 camR = _camera.transform.right;
            camF = new Vector3(camF.x, 0, camF.z).normalized;
            camR = new Vector3(camR.x, 0, camR.z).normalized;
            

            if (isOnGround == true)
            {
                moveInput = new Vector2(Input.GetAxis("HorizontalKey"), Input.GetAxis("VerticalKey"));
            }

            Vector3 heading = new Vector3(moveInput.x, 0.0f, moveInput.y);
            Vector3 headingRotated = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * heading;

            if (isOnGround == true && heading != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(headingRotated.normalized), 0.2f);
                transform.position += (camF*moveInput.y + camR*moveInput.x) * Time.deltaTime * moveSpeed;
            } else {
                
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
    }
}