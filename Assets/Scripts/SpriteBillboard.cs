using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController
{
    public class SpriteBillboard : MonoBehaviour
    {
        [Header("Billboarding Variables")]
		protected GameObject _camera;

		void Awake()
		{
			// Gets the component CameraController from the Main Camera object.
			_camera = GameObject.Find("Main Camera");

		}

		void Update()
		{
			BillboardSprite();
		}

		void LateUpdate()
		{
			BillboardSprite();
		}

		// Ensures that the billboard is always pointed at camera in x and z space.
		protected void BillboardSprite()
		{
            Vector3 newRotation = _camera.transform.eulerAngles;
            newRotation = new Vector3(newRotation.x, newRotation.y, 0);
            transform.eulerAngles = newRotation;
        
		}
    }
}