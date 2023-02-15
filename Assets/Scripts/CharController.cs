using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController
{
    public class CharController : MonoBehaviour
    {
        // Sets character's movespeed.
        public float moveSpeed;
        public float maxSpeed;
        public float jumpSpeed;
        public float jumpForce;
        private float momentum;
        private Vector3 charDirection;

        // Sets variable for character's rigidbody
        private Rigidbody playerRb;

        // Creates a transform named _t
        private Transform _t;

        // Creates a Vector2 named charRotation
        public Vector2 charRotation;

        // Creates 2 Vector 3s named Forward and Right respectively.
        public Vector3 forward, right;

        // Character States
        public bool isOnGround;

        // Start is called before the first frame update
        void Start()
        {
            _t = transform;
            forward = Camera.main.transform.forward;
            forward.y = 0;
            forward = Vector3.Normalize(forward);
            right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
            playerRb = GetComponent<Rigidbody>();
            isOnGround = true;
        }

        // Update is called once per frame
        void Update()
        {
            if ((Input.GetAxis("HorizontalKey") != 0 || Input.GetAxis("VerticalKey") != 0))
            {
                charDirection = Move();
            }

            if (isOnGround == true && Input.GetKeyDown(KeyCode.Space)){
                Vector3 direction = new Vector3(Input.GetAxis("HorizontalKey"), 0, Input.GetAxis("VerticalKey"));
                Vector3 rightMovement = right * moveSpeed * Time.deltaTime * Input.GetAxis("HorizontalKey");
                Vector3 upMovement = forward * moveSpeed * Time.deltaTime * Input.GetAxis("VerticalKey");

                Vector3 heading = Vector3.Normalize(rightMovement + upMovement);
                momentum = playerRb.velocity.magnitude;

                playerRb.AddForce((Vector3.up * jumpForce), ForceMode.Impulse);
                playerRb.AddForce(charDirection * jumpSpeed, ForceMode.Impulse); // Find a way to replace the static jumpSpeed variable with the character's momentum at time of jump (currently momentum is reduced to 0 when the chara)
                isOnGround = false;
                Debug.Log(isOnGround);
            }

        
        /* 
        This code is from the Jump in the ball-island game, from the Junior Programmer course.
        See if the jump code is useful for our purposes here.
        IEnumerator Smash()
        {
            var enemies = FindObjectsOfType<Enemy>();

            // Store the Y position before taking off
            floorY = transform.position.y;

            // Calculate the amount of time we will go up for
            float jumpTime = Time.time + hangTime;

            while(Time.time < jumpTime)
            {
                // Move the player up while still keeping their X velocity
                playerRb.velocity = new Vector2(playerRb.velocity.x, smashSpeed);
                yield return null;
            }

            playerUp = true;

            // Now move the Player down
            while(transform.position.y > floorY)
            {
                playerRb.velocity = new Vector2(playerRb.velocity.x, -smashSpeed * 2);
                yield return null;
            }

            // Cycle through all enemies
            for (int i = 0; i < enemies.Length; i++)
            {
                // Apply an expl;osion force that originates from our position.
                if(enemies[i] != null)
                    enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.0f, ForceMode.Impulse);
            }

            // Smash is over, set boolean to false
            smashing = false;
        } */
            
            
        }

        private Vector3 Move()
        {
            Vector3 direction = new Vector3(Input.GetAxis("HorizontalKey"), 0, Input.GetAxis("VerticalKey"));
            Vector3 rightMovement = right * moveSpeed * Time.deltaTime * Input.GetAxis("HorizontalKey");
            Vector3 upMovement = forward * moveSpeed * Time.deltaTime * Input.GetAxis("VerticalKey");

            Vector3 heading = Vector3.Normalize(rightMovement + upMovement);

            //momentum = playerRb.velocity.magnitude;

            if (isOnGround == true)
            {
                transform.forward = heading;
                transform.position += rightMovement;
                transform.position += upMovement;
                charRotation = _t.eulerAngles;
            } else {

            }
            return heading;
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