using UnityEngine;
using System.Collections;

namespace SpriteController
{
	public class CameraController : MonoBehaviour
	{
		// Defines various parameters for the controller, starting with Target, AKA the central object the camera rotates around
		public Transform target;
		// ...the X rotation of the camera...
		public float angleY = 35;
		// ...the rotation smoothing
		public float rotationSmoothing = 10;
		// ... the rotation sensitivity...
		public float rotationSensitivity = 7;
		// ...and the Z-distance from the target.
		public float distance = 10;

		// Defines a new Vector3 named _angle
		private Vector3 _angle = new Vector3();

		// Defines a new Quaternion named _oldRotation
		private Quaternion _oldRotation = new Quaternion();

		// Defines a new Transform named _t
		private Transform _t;

		// Defines a Vector2 named CurrentRotation using the values of _angle
		public Vector2 CurrentRotation { get { return _angle; } }

		// At startup
		void Start()
		{
			// Sets _t to equal the current transform of this game object (the Camera)
			_t = transform;
			
			// Sets _oldRotation to equal the current rotation of _t 
			_oldRotation = _t.rotation;

			// Sets the Y value of the _angle Vector3 to equal the value of the angleY float
			_angle.y = angleY;
		}

		// Every frame
		void Update()
		{
			// If target has a value AND RMB is held down
			if(target && Input.GetMouseButton(1))
			{
				// Then set _angle's x value to = current value + (Mouse input on the X axis multiplied by rotationSensitivity); ie: add mouse input to current position to calculate a new angle.
				_angle.x += Input.GetAxis("Mouse X") * rotationSensitivity;
				// Apply the ClampAngle Method to the updated angle to ensure it falls within -180 - 180 degree range.
				SpriteTools.ClampAngle(ref _angle);
			}
		}

		// After all Update methods are processed
		void LateUpdate()
		{
			// if target (ie the player, focal point of camera) is set
			if(target)
			{
				// Then set a Quaternion named angleRotation where the x value = the y value of _angle, the y value = the x value of _angle, and the z value = 0. 
				// I know, it's confusing that they're swapped but it makes sense if you think about it (a rotation in the X plane is a rotation AROUND Y.)
				Quaternion angleRotation = Quaternion.Euler(_angle.y, _angle.x, 0);
				// Sets the Quaternion currentRotation to = a quaternion interpolated between _oldRotation and angleRotation over a period of time equal to the product of Time.deltaTime multiplied by rotationSmoothing.
				Quaternion currentRotation = Quaternion.Lerp(_oldRotation, angleRotation, Time.deltaTime * rotationSmoothing);

				// Replaces the previous value of oldRotation with the newly calculated currentRotation
				_oldRotation = currentRotation;

				// Changes _t's positon coordinates, thereby changing the position of the camera
				_t.position = target.position - currentRotation * Vector3.forward * distance;
				// Changes _t's rotation coordinates such that the camera is looking in the right direction, and its "up" is aligned correctly
				_t.LookAt(target.position, Vector3.up);
			}
		}
	}
}