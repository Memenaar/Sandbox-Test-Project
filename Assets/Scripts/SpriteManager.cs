using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Attach this script directly to a Billboard sub-object to ensure it:
// 1. Rotates to always faces the camera
// 2. Uses the correct sprite relative to the facing direction of the in-game object and angle of the camera.

namespace SpriteController
{
	public class SpriteManager : MonoBehaviour
	{
		[Header("Billboarding Variables")]
		protected GameObject _cameraDirection;
		private Transform _billboard;
		private Transform _objTransform;
		public Vector3 targetPoint;

		[Header("Rotation Variables")]
		// Stores the CameraController script.
		protected CameraController _cameraController;
		
		// List of basic directional sprites.
		// Original Author's note: I don't recommend using a list like this, it's just for example
		public List<Sprite> directionSprites = new List<Sprite>();
		
		// Store the sprite renderer.
		private SpriteRenderer _sprite;
		
		// The facing direction of the billboard itself. Always down, because the billboard faces the Camera.
		private Facing _bbFacing = Facing.Down;

		// The direction that the game object is facing relative to the camera.
		private Facing _relFacing = Facing.Up;

		void Awake()
		{
			/* Initializes the variables for billboarding */
			// Sets _t to equal the transform values of the attached game object (in this case, Player)
			_objTransform = transform.parent.transform;

			// Gets the transform value of _sprite and stores it in _billboard
			_billboard = gameObject.transform;

			/* Initializes variables for sprite angling */
			// Gets the SpriteRenderer component attached to this object and stores it in _sprite
			_sprite = GetComponentInChildren<SpriteRenderer>();

			// Gets the component CameraController from the Main Camera object.
			_cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();

			// Gets the component CameraController from the Main Camera object.
			_cameraDirection = GameObject.Find("Main Camera");
		}

		void Update()
		{
			BillboardSprite();
		}

		void LateUpdate()
		{
			DetermineDirection();
			AngleSprite();
		}

		// Ensures that the billboard is always pointed at camera in x and z space.
		void BillboardSprite()
		{
			// Creates a new Vector3 using the Main Camera's x and z values, and the game object's y value, then subtracts the game object's transform from that Vector3 and stores the remaining values in targetPoint.
			targetPoint = new Vector3(_cameraDirection.transform.position.x, _cameraDirection.transform.position.y, _cameraDirection.transform.position.z) - _objTransform.position;
			// Sets billboard's y rotation so that x matches camera rotation (ie the billboard is always facing the camera)
			_billboard.rotation = Quaternion.LookRotation(-targetPoint, Vector3.up);
			//NOTE: may want to figure out how to clamp billboard X rotation @ camera angle (eg: 35 deg)
		}

		// Determines the direction of the game object relative to the camera in x and z space.
		void DetermineDirection()
		{
			Vector2 _objRotation = _objTransform.eulerAngles;
			// Defines a float named rX and sets it to equal the X component of the camera's current rotation.
			float rX = _cameraController.CurrentRotation.x - _objRotation.y;

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
				_relFacing = Facing.Down;
			}
			// else if the value of x is less than 67.5f...
			else if(x < 67.5f)
			{
				// if rX is less than 0, set the value of _facing to DownRight. Else set the value of _facing to DownLeft. 
				_relFacing = rX < 0 ? Facing.DownRight : Facing.DownLeft;
			}
			else if(x < 112.5f)
			{
				// if rX is less than 0, set the value of _facing to Right. Else set the value of _facing to Left.
				_relFacing = rX < 0 ? Facing.Right : Facing.Left;
			}
			else if(x < 157.5f)
			{
				// if rX is less than 0, set the value of _facing to UpRight. Else set the value of _facing to UpLeft.
				_relFacing = rX < 0 ? Facing.UpRight : Facing.UpLeft;
			}
			else
			{
				// set the value of _facing to Up.
				_relFacing = Facing.Up;
			}
		}

		// Sets the active sprite displayed by the sprite renderer to the appropriate angle, based on the game object's direction relative to the camera. 
		void AngleSprite()
		{
			// Reset the flipX value of _sprite to false.
			_sprite.flipX = false;

			// Creates and sets the offset integer to equal the value of _facing minus the value of Facing as defined in _cameraDirection.
			// Essentially set offset to the value of billboard direction minus camera direction.
			int offset = _bbFacing - _relFacing;

			Debug.Log("Offset before: " + offset);
			// If offset less than 0...
			if(offset < 0)
			{
				// ...then set offset to = offset + 8;
				// ie: if offset is negative, converts it to its positive equivalent.
				offset += 8;
			}
			Debug.Log("Offset after: " + offset);

			// Brackets are manual or "explicit" casting; used to convert a larger size data type to a smaller size data type.
			// In this instance, if I understand correctly, we're creating a Facing enum variable named direction and setting it to = the value of offset, converted from an int to an enum?
			Facing direction = (Facing)offset;

			// Checks direction for any of the following values
			switch(direction)
			{
				// If direction = Facing.DownRight, set direction = Facing.DownLeft and flip the sprite along its X axis
				case Facing.DownRight:
					direction = Facing.DownLeft;
					_sprite.flipX = true;
					break;
				// If direction = Facing.Right, set direction = Facing.Left and flip the sprite along its X axis
				case Facing.Right:
					direction = Facing.Left;
					_sprite.flipX = true;
					break;
				// If direction = Facing.UpRight, set direction = Facing.UpLeft and flip the sprite along its X axis
				case Facing.UpRight:
					direction = Facing.UpLeft;
					_sprite.flipX = true;
					break;
			}

			// Original author's note: "Change this with your own sprite animation stuff"
			// Not sure how this works yet but I'll figure it out or replace it.
			// NOTE: does not effect billboarding. Controls direction of sprite/sprite update.
			_sprite.sprite = directionSprites[(int)direction];

		}
	}
}