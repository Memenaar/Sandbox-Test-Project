using UnityEngine;
using System.Collections;

namespace SpriteController
{
	public class CameraController : MonoBehaviour
	{

		private PlayerActions _playerActions;

		// Defines various parameters for the controller, starting with Target, AKA the central object the camera rotates around
		public Transform target;
		// ...the X rotation of the camera...
		private const float _angleY = 40;
		// ...the rotation smoothing
		private const float cameraSmoothing = 10;
		// ... the rotation sensitivity...
		private const float rotationSensitivity = 30;
		// ...and the Z-distance from the target.
		private float _distance = 20;
		// Used to track how long the camera rotate action has been active, increasing speed over time.
		private float _rotTimer = 0f;

		// Defines a new Vector3 named _angle
		private Vector3 _angle = new Vector3();

		// Defines a new Quaternion named _oldRotation
		private Quaternion _oldRotation = new Quaternion();

		private Vector3 _oldPosition;
		private Vector3 _currentPosition;

		// Defines a new Transform named _t
		private Transform _t;

		// Defines a Vector2 named CurrentRotation using the values of _angle
		public Vector2 CurrentRotation { get { return _angle; } }

		void Awake()
		{
			// Player Action initialization
            _playerActions = new PlayerActions();
		}

		// At startup
		void Start()
		{
			// Sets _t to equal the current transform of this game object (the Camera)
			_t = transform;
			
			// Sets _oldRotation to equal the current rotation of _t 
			_oldRotation = _t.rotation;
			_oldPosition = _t.position;
			// Sets the Y value of the _angle Vector3 to equal the value of the _angleY float
			_angle.y = _angleY;
		}

		// Every frame
		void Update()
		{
			CamRotation();
			CamZoom();
		}

		private void OnEnable()
		{
			_playerActions.CameraActions.Enable();
		}

		private void OnDisable()
		{
			_playerActions.CameraActions.Disable();
		}

		// After all Update methods are processed
		void LateUpdate()
		{
			CamMovement();
		}

		void CamRotation()
		{
			// If target has a value AND RMB is held down
			float cameraInput = _playerActions.CameraActions.CameraRotation.ReadValue<float>();
			if(target && cameraInput != 0)
			{
				_rotTimer += Time.deltaTime;
				_rotTimer = Mathf.Clamp(_rotTimer, 0f, 3.0f);
				// Then set _angle's x value to = current value + (Mouse input on the X axis multiplied by rotationSensitivity); ie: add mouse input to current position to calculate a new angle.
				_angle.x += cameraInput * _rotTimer * rotationSensitivity * Time.deltaTime;
				// Apply the ClampAngle Method to the updated angle to ensure it falls within -180 - 180 degree range.
				SpriteTools.ClampAngle(ref _angle);
			}
			else 
			{
				_rotTimer = 0f;
			}
		}

		void CamZoom()
		{
			float cameraZoom = _playerActions.CameraActions.CameraZoom.ReadValue<float>();
			if(target && cameraZoom != 0)
			{
				_distance += cameraZoom * (rotationSensitivity * 0.5f) * Time.deltaTime;
				_distance = Mathf.Clamp(_distance, 10.0f, 30.0f);
			}
		}

		void CamMovement()
		{
			// if target (ie the player, focal point of camera) is set
			if(target)
			{
				// Then set a Quaternion named angleRotation where the x value = the y value of _angle, the y value = the x value of _angle, and the z value = 0. 
				// I know, it's confusing that they're swapped but it makes sense if you think about it (a rotation in the X plane is a rotation AROUND Y.)
				Quaternion angleRotation = Quaternion.Euler(_angle.y, _angle.x, 0);
				// Sets the Quaternion currentRotation to = a quaternion interpolated between _oldRotation and angleRotation over a period of time equal to the product of Time.deltaTime multiplied by rotationSmoothing.
				Quaternion currentRotation = Quaternion.Lerp(_oldRotation, angleRotation, Time.deltaTime * cameraSmoothing);
				// Replaces the previous value of oldRotation with the newly calculated currentRotation
				_oldRotation = currentRotation;

				// Sets current position of the camera
				_currentPosition = (target.position - currentRotation * Vector3.forward * _distance);
				// Changes _t's positon coordinates, thereby changing the position of the camera
				_t.position = Vector3.Lerp(_oldPosition, _currentPosition, Time.deltaTime * cameraSmoothing);
				// Replaces the previous value of oldPosition with the newly calculated current rotation.
				_oldPosition = _currentPosition;

				// Changes _t's rotation coordinates such that the camera is looking in the right direction, and its "up" is aligned correctly
				_t.LookAt(target.position, Vector3.up);
			}
		}
	}
}