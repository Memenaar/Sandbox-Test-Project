using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController
{
    public class SpriteAnimation : MonoBehaviour
    {
            public Rigidbody charBody;
            public SpriteRenderer spriteRenderer;

            [Header("Walk Sprites")]
            // Create a public list of walking sprites for each direction. 
            public List<Sprite> nWalkSprites;
            public List<Sprite> nwWalkSprites;
            public List<Sprite> wWalkSprites;
            public List<Sprite> swWalkSprites;
            public List<Sprite> sWalkSprites;

            [Header("Jump Sprites")]
            // Create a public list of walking sprites for each direction. 
            public List<Sprite> nJumpSprites;
            public List<Sprite> nwJumpSprites;
            public List<Sprite> wJumpSprites;
            public List<Sprite> swJumpSprites;
            public List<Sprite> sJumpSprites;
        
            public CharController charControllerScript; // Take the CharController script and store it in a variable.
            public float frameRate; // Animation frame rate.
            float idleTime; // 

            private Facing spriteDirection = Facing.Up; // Define a Facing named spriteDirection, and set the value to Up by default.
            List<Sprite> selectedAnimation = null; // Define a list of Sprites named selectedAnimation and set it to null to begin.

        // Start is called before the first frame update
        void Awake()
        {
            charControllerScript = GetComponentInParent<CharController>();
        }

        void Start()
        {
        }


        // Update is called once per frame
        void Update()
        {

            getSpriteDirection();
            setAnimSprite();
            //Debug.Log("Sprite Direction: " + spriteDirection);
            //Debug.Log(("Horizontal Input: " + Input.GetAxis("HorizontalKey")+ ", Vertical Input: " + Input.GetAxis("VerticalKey")));
            // charControllerScript.moveSpeed = 6; // changes the value of the moveSpeed variable in the CharController script
        }


        /* METHODS */

        // Gets the current Facing direction of the sprite from the CameraDirection script.
        void getSpriteDirection() 
        {
            //spriteDirection = gameObject.GetComponent<SpriteManager>()._relFacing;
        }

        // Determines the selected animation to be played by the sprite renderer 
        //Likely in the future want to change this to be "animation direction", have a seperate method that handles character state, and have this method interpret that character state to choose which sprite set it pulls from
        List<Sprite> setAnimation()
        {
            if (charControllerScript.isOnGround == true && (Input.GetAxis("HorizontalKey") != 0) | (Input.GetAxis("VerticalKey") != 0)) // Check whether input is being received by the Horizontal or Vertical movement keys.
            {
                // If the above is true, check spriteDirection and set the animation accordingly.
                if(spriteDirection == Facing.Down){
                    selectedAnimation = sWalkSprites;
                } else if (spriteDirection == Facing.DownLeft) {
                    selectedAnimation = swWalkSprites; 
                } else if (spriteDirection == Facing.Left) {
                    selectedAnimation = wWalkSprites; 
                } else if (spriteDirection == Facing.UpLeft) {
                    selectedAnimation = nwWalkSprites; 
                } else if (spriteDirection == Facing.Up) {
                    selectedAnimation = nWalkSprites; 
                } else if (spriteDirection == Facing.UpRight) {
                    selectedAnimation = nwWalkSprites;
                    spriteRenderer.flipX = true;
                } else if (spriteDirection == Facing.Right) {
                    selectedAnimation = wWalkSprites;
                    spriteRenderer.flipX = true;
                } else if (spriteDirection == Facing.DownRight) {
                    selectedAnimation = swWalkSprites;
                    spriteRenderer.flipX = true;    
                }
            } else if (charControllerScript.isOnGround == false) {
                if(spriteDirection == Facing.Down){
                    selectedAnimation = sJumpSprites;
                } else if (spriteDirection == Facing.DownLeft) {
                    selectedAnimation = swJumpSprites; 
                } else if (spriteDirection == Facing.Left) {
                    selectedAnimation = wJumpSprites; 
                } else if (spriteDirection == Facing.UpLeft) {
                    selectedAnimation = nwJumpSprites; 
                } else if (spriteDirection == Facing.Up) {
                    selectedAnimation = nJumpSprites; 
                } else if (spriteDirection == Facing.UpRight) {
                    selectedAnimation = nwJumpSprites;
                    spriteRenderer.flipX = true;
                } else if (spriteDirection == Facing.Right) {
                    selectedAnimation = wJumpSprites;
                    spriteRenderer.flipX = true;
                } else if (spriteDirection == Facing.DownRight) {
                    selectedAnimation = swJumpSprites;
                    spriteRenderer.flipX = true;    
                }
            } else {
                    selectedAnimation = null; // If there is no horizontal or vertical key input, set the selected animation to null
            }
            return selectedAnimation;
        }

        // Takes the result of SetAnimation, and changes animates the sprite renderer based on the result
        void setAnimSprite()
        {
            List<Sprite> directionAnimation = setAnimation();
            if(directionAnimation != null){ // if holding a direction
                float playTime = Time.time - idleTime; // time since we started walking
                int totalFrames = (int)(playTime * frameRate); // total frames since we started
                int frame = totalFrames % directionAnimation.Count; //current frame

                spriteRenderer.sprite = directionAnimation[frame]; //sets the sprite in the sprite renderer
            } else { // if holding nothing, ie: input is neutral.
                idleTime = Time.time;
            }

        }

        
    }
}