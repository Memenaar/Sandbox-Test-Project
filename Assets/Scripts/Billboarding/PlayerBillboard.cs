using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController {
    public class PlayerBillboard : CharBillboard
    {

        // Start is called before the first frame update
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
        }

    }
}