using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController {
    public class CharBillboard : SpriteBillboard
    {
    	[Header("Rotation Variables")]
		// Stores the CameraController script.
		protected CameraController _cameraController;

		// Store the transform of the character's billboard game object
        protected Transform _billboard;
		// Store the Transform of the character's Navigator game object.
        protected Transform _navigator;

        // Store the sprite renderer.
		protected SpriteRenderer _renderer;

		// List of basic directional sprites.
		// Original Author's note: I don't recommend using a list like this, it's just for example
		public List<Sprite> directionSprites = new List<Sprite>();

		// Sets the initial facing direction of the character sprite, relative to the camera.
        protected Facing _spriteFacing = Facing.Up;
		// Sets the facing direction of the billboard itself.
        protected Facing _bbFacing = Facing.Down;

        void Awake()
        {
			GetCamera();
			InitializeBillboard();
        }

        void Update()
		{	
		}

        void LateUpdate()
        {
            DetermineDirection();
			AngleSprite();
        }

		// Initializes all the variables used for a standard Character Billboard
		protected void InitializeBillboard()
		{
			// Sets _t to equal the transform values of the attached game object (in this case, Player)
			_navigator = transform.parent.Find("Navigator").transform;

			// Gets the transform value of the game object and stores it in _billboard
			_billboard = gameObject.transform;

			/* Initializes variables for sprite angling */
			// Gets the SpriteRenderer component attached to this object and stores it in _sprite
			_renderer = GetComponent<SpriteRenderer>();
			// Gets the component CameraController from the Main Camera object.
			_cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
		}

        // Determines the direction the character's Navigator is facing relative to the camera in x and z space.
		protected void DetermineDirection()
		{
			Vector2 _navRotation = _navigator.eulerAngles;

			// Defines a float named rX and sets it to equal the X component of the camera's current rotation minus the rotation of the navigator.
			float rX = _cameraController.CurrentRotation.x - _navRotation.y;

			// Ensure that the product of the previous equation does not exceed -180 or 180 in either direction
			// If the value of rX is less than -180, change the value of rX to rX+360
			if(rX < -180) rX += 360;
			// if the value of rX is greater than 180, change the value of rX to rX-360
			else if(rX > 180) rX -= 360;

			// Defines a float named x and sets it to = the absolute value of rX. ie: set that number to positive if negative.
			float x = Mathf.Abs(rX);
			
			// The following IF statement changes the _facing enum, which is used by the SpriteManager script to determine the direction the sprite should be facing..
			// if the value of x is less than 22.5f...
			if(x < 22.5f)
			{
				// ...then set the value of _relFacing to Down
				_spriteFacing = Facing.Down;
			}
			// else if the value of x is less than 67.5f...
			else if(x < 67.5f)
			{
				// if rX is less than 0, set the value of _facing to DownRight. Else set the value of _facing to DownLeft. 
				_spriteFacing = rX < 0 ? Facing.DownRight : Facing.DownLeft;
			}
			else if(x < 112.5f)
			{
				// if rX is less than 0, set the value of _facing to Right. Else set the value of _facing to Left.
				_spriteFacing = rX < 0 ? Facing.Right : Facing.Left;
			}
			else if(x < 157.5f)
			{
				// if rX is less than 0, set the value of _facing to UpRight. Else set the value of _facing to UpLeft.
				_spriteFacing = rX < 0 ? Facing.UpRight : Facing.UpLeft;
			}
			else
			{
				// set the value of _facing to Up.
				_spriteFacing = Facing.Up;
			}
		}

		// Sets the active sprite displayed by the sprite renderer to the appropriate angle, based on the game object's direction relative to the camera. 
		protected void AngleSprite()
		{
			// Reset the flipX value of _sprite to false.
			_renderer.flipX = false;

			// Creates and sets the offset integer to equal the value of _facing minus the value of Facing as defined in _cameraDirection.
			// Essentially set offset to the value of billboard direction minus camera direction.
			int offset = _bbFacing - _spriteFacing;

			// If offset less than 0...
			if(offset < 0)
			{
				// ...then set offset to = offset + 8;
				// ie: if offset is negative, converts it to its positive equivalent.
				offset += 8;
			}

			// Brackets are manual or "explicit" casting; used to convert a larger size data type to a smaller size data type.
			// In this instance, if I understand correctly, we're creating a Facing enum variable named direction and setting it to = the value of offset, converted from an int to an enum?
			Facing direction = (Facing)offset;

			// Checks direction for any of the following values
			switch(direction)
			{
				// If direction = Facing.DownRight, set direction = Facing.DownLeft and flip the sprite along its X axis
				case Facing.DownRight:
					direction = Facing.DownLeft;
					_renderer.flipX = true;
					break;
				// If direction = Facing.Right, set direction = Facing.Left and flip the sprite along its X axis
				case Facing.Right:
					direction = Facing.Left;
					_renderer.flipX = true;
					break;
				// If direction = Facing.UpRight, set direction = Facing.UpLeft and flip the sprite along its X axis
				case Facing.UpRight:
					direction = Facing.UpLeft;
					_renderer.flipX = true;
					break;
			}

            // Original author's note: "Change this with your own sprite animation stuff"
			// Not sure how this works yet but I'll figure it out or replace it.
			// NOTE: does not effect billboarding. Controls direction of sprite/sprite update.
			//_renderer.sprite = directionSprites[(int)direction];
        }
    }
}