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
			GetCamera();
		}

		void Update()
		{
		}

		void LateUpdate()
		{
		}

		// Gets the main camera and initializes the _camera variable for all Sprite Billboards.
		protected void GetCamera()
		{
			_camera = GameObject.Find("Main Camera");
		}

    }
}