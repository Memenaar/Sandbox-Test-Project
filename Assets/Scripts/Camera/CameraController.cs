using System;
using UnityEngine;
using System.Collections;
using SpriteController;

	public class CameraController : MonoBehaviour
	{

		// Objects & Components
		public InputReader inputReader; // Scriptable object that conveys input
		public Transform target; // Game object of camera's focus.
		private Transform _t; // Camera's transform.

		// Basic Values
		private const float _angleY = 40; // Camera's x rotation
		private const float cameraSmoothing = 10; // Camera smoothing
		private const float maxCamSpeed = 1; // Max zoom/rotation speed
		public const float baseSensitivity = 0.5f; // Starting multiplier for zoom and rotation
		public const float timerReset = 0.02f; // Reset value for zoom and rotation timers
		public bool _cameraLock = false; // is camera movement locked.
		
		// Movement Values	
		private Quaternion _oldRotation = new Quaternion(); // Stores prior camera position for use in MoveCamera()
		private Vector3 _angle = new Vector3(); // Stores changes to the camera position before they are applied in MoveCamera()
		private Vector3 _oldPosition; 
		private Vector3 _currentPosition;

		// Zoom Values
		private float _distance = 20; // Z-distance from the target.
		public float _zoomDir = 0; // Tracks direction of Zoom input
		public float _zoomTimer = timerReset; // Increases while zoom input is received
		public float _zoomTimeout = 0f; // Tracks time between scroll wheel inputs when using mouse
		public bool _mouseZoom = false; // Tracks whether zoom input is being received via mouse
		private bool _isRMBPressed; // Tracks whether the RMB is being held down.
		private Vector2 _zoomInput = Vector2.zero; // Stores zoom input before it is applied

		// Rotation Values
		public float _rotDir = 0; // Tracks direction of rotation input
		public float _rotTimer = timerReset; // Increases while rotation input is received
		private Vector2 _rotationInput = Vector2.zero; // Stores rotation input before it is applied


		// Getters & Setters
		public Vector2 CurrentRotation { get { return _angle; } }
		public bool CameraLock { get { return _cameraLock; } set { _cameraLock = value; }}


		void Awake()
		{
			inputReader.GameInput.Camera.Enable(); // Want to move this to Game State stuff eventually.
			InitializeCamera();
		}

		void Start()
		{
		}

		void Update()
		{
			ProcessRotation();
			ProcessZoom();
		}

		void LateUpdate()
		{
			ApplyRotation();
			ApplyZoom();
			MoveCamera();
		}

		private void OnEnable()
		{
			inputReader.CameraRotateEvent += CamRotation;
			inputReader.CameraZoomEvent += CamZoom;
			inputReader.EnableMouseControlCameraEvent += OnEnableMouseControlCamera;
			inputReader.DisableMouseControlCameraEvent += OnDisableMouseControlCamera;

		}

		private void OnDisable()
		{
			inputReader.CameraRotateEvent -= CamRotation;
			inputReader.CameraZoomEvent -= CamZoom;
			inputReader.EnableMouseControlCameraEvent -= OnEnableMouseControlCamera;
			inputReader.DisableMouseControlCameraEvent -= OnDisableMouseControlCamera;
		}

		// Initialization
		private void InitializeCamera()
		{
			_t = transform; // Sets _t to equal the current transform of this game object (the Camera)
			_oldRotation = _t.rotation; // Sets _oldRotation to equal the current rotation of _t 
			_oldPosition = _t.position; // Sets _oldPosition to equal the current position of _t 
			_angle.y = _angleY; // Sets the Y value of the _angle Vector3 to equal the value of the _angleY float
		}

		// ROTATION
		private void CamRotation(Vector2 cameraRotation, bool isDeviceMouse)
		{
			if (_cameraLock) // If camera is locked, abort
				return;

			if (isDeviceMouse && !_isRMBPressed) // if the input device is a mouse and RMB is not pressed, abort
				return;
			
			if (target && Mathf.Abs(cameraRotation.x) >= 0.5f) // if the camera has a target, and rotation input is >= 0.5
			{
				_rotDir = Mathf.Sign(cameraRotation.x); // then set rotation direction to -1 or 1 according to the input
			} else
			{
				_rotDir = 0; // Otherwise set rotation direction to 0
			}
		}

		private void ProcessRotation()
		{
			_rotationInput.x = _rotDir * baseSensitivity * _rotTimer; // rotation input = direction of rotation x base sensitivity x rotation timer
			_rotationInput.x = Mathf.Clamp(_rotationInput.x, -maxCamSpeed, maxCamSpeed); // clamp the rotation input to maxcamspeed in either direction
			RotationTimer();
		}

		private void RotationTimer()
		{
			if (_isRMBPressed) // if RMB is pressed
			{
				if (Mathf.Sign(_rotationInput.x) == _rotDir) _rotTimer = Mathf.Lerp(_rotTimer, 2f, Time.deltaTime); // if rotation input and rotDir are in the same direction, increment rotation timer as normal
				else _rotTimer = timerReset; // otherwise reset timer
			} else // if RMB not pressed
			{
				if (_rotationInput.x != 0) _rotTimer = Mathf.Lerp(_rotTimer, 2f, Time.deltaTime); // if rotation input is not 0, increment timer as normal
				else _rotTimer = timerReset; // otherwise reset timer
			}
		}

		private void ApplyRotation()
		{
			if (_rotationInput.x != 0) // if rotation input is not 0
			{
				_angle.x += _rotationInput.x; // add rotation input to _angle's x vector, to be applied during MoveCamera()
				SpriteTools.ClampAngle(ref _angle); // Apply the ClampAngle Method from SpriteTools to ensure it falls within -180 - 180 degree range (prevent sprite rotation from breaking)/
			} 
		}

		private void OnEnableMouseControlCamera() // fires when RMB is pressed down
		{
			_isRMBPressed = true; // marks RMB as pressed

			Cursor.lockState = CursorLockMode.Locked; // Locks cursor
			Cursor.visible = false; // Makes cursor invisible

			StartCoroutine(DisableMouseControlForFrame()); // Runs coroutine below, locks input for the frame.
		}

		IEnumerator DisableMouseControlForFrame()
		{		
			_cameraLock = true;
			yield return new WaitForEndOfFrame();
			_cameraLock = false;
		}

		private void OnDisableMouseControlCamera() // fires when RMB is released
		{
			_isRMBPressed = false; // marks RMB as not pressed

			Cursor.lockState = CursorLockMode.None; // Unlocks cursor
			Cursor.visible = true; // Makes cursor visible

			_rotationInput.x = 0; // clear rotation input
		}

		// ZOOM
		private void CamZoom(Vector2 cameraZoom, bool isDeviceMouse)
		{
			if (_cameraLock) // If camera movement is locked abort
				return;

			if (isDeviceMouse) _mouseZoom = true; // if the input device is a mouse set _mouseZoom to true.
			else _mouseZoom = false; // else set _mouseZoom to false
			
			if(target && Mathf.Abs(cameraZoom.y) >= 0.5f) // if camera has a target and received input is greater than or equal to 0.5f
			{
				_zoomDir = Mathf.Sign(cameraZoom.y); // Then set the _zoomDir to 1 or -1 based on direction of input.
			} else
			{
				_zoomDir = 0; // Otherwise set _zoomDir to 0.
			}
		}

		private void ProcessZoom()
		{
			_zoomInput.y = _zoomDir * baseSensitivity * _zoomTimer; // applied input = direction of input x base movement x zoom timer
			_zoomInput.y = Mathf.Clamp(_zoomInput.y, -maxCamSpeed, maxCamSpeed); // Clamps input to maxCamSpeed in either direction
			ZoomTimer();
		}

		private void ZoomTimer()
		{
			if (_mouseZoom) // if Input device is a mouse (ie: scroll wheel)
			{
				if (_zoomInput.y != 0) {_zoomTimer = Mathf.Lerp(_zoomTimer, 2f, Time.deltaTime); _zoomTimeout = 0f;} // if scroll wheel input != 0, increment timer as normal
				else 
				{
					if (_zoomTimeout <= 0.25f) // if input = 0 && the timeout counter is less than 0.5f
					{
						_zoomTimer = Mathf.Lerp(_zoomTimer, 2f, Time.deltaTime); // Increment timer as normal
						_zoomTimeout += Time.deltaTime; // then increment the timeout counter
					} else // if timeout counter is 0.5f or greater
					{
						_zoomTimer = timerReset; // reset zoom timer
						_zoomTimeout = 0f; // reset timeout counter
						_mouseZoom = false; // mark input as no longer coming from mouse
					}
				}
			}
			else // if input device is not mouse
			{
				if (_zoomInput.y != 0) _zoomTimer = Mathf.Lerp(_zoomTimer, 2f, Time.deltaTime); // increment timer so long as input is not 0
				else _zoomTimer = timerReset; // otherwise reset zoom timer
			}
		}

		private void ApplyZoom()
		{
			if (_zoomInput.y != 0) // if _zoomInput's y value is not 0
			{
				_distance += _zoomInput.y; // then add it to the camera's distance from target
				_distance = Mathf.Clamp(_distance, 10.0f, 30.0f); // Ensures distance is not less than 10 or greater than 30.
			} 
		}

		// MOVEMENT
		void MoveCamera()
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