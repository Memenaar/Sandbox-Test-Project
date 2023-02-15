using UnityEngine;
using System.Collections;

namespace SpriteController
{
	/* public class CameraDirection : MonoBehaviour
	{
		// Creates a field storing information from the CameraController script.
		protected CameraController _camera;
		
		// Creates a field storing the Facing enum, and sets its value to Facing.Up.
		// This variable tracks the direction that the game object is facing relative to the camera.
		protected Facing _relFacing = Facing.Up;

		// Sets an enum called Facing that can be redefined in subordinate classes?
		// Still don't totally get the Virtual thing but I think it's an alternate version of _facing that is safe to be manipulated.
		// Sets value to = the value of _relFacing.
		public virtual Facing relFacing { get { return _relFacing; } }
		// Sets a Vector2 named Angle and sets that value to the current rotation of the Camera as defined in the CameraController.
		public virtual Vector2 Angle { get { return _camera.CurrentRotation; } }

		public Facing visibleFacing;

		protected Vector3 objRotation;

		// Use of Awake() runs commands before the script is even called.
		// Used to initialize things that another script might call on before this one is run.
		public virtual void Awake()
		{
			// Gets the component CameraController (this is possible because both scripts are attached to the game object, ie the Camera)
			_camera = GetComponent<CameraController>();

		}

	}*/
}