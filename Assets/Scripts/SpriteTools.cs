using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines a new namespace "SpriteController"
namespace SpriteController
{

	// Defines a new class named SpriteTools
	public static class SpriteTools
	{

		// Defines a new method named ClampAngle, which requires input in the form of a Vector3. Internally, the method will refer to the input Vector3 as "angle". The "ref" keyword ensures the Method changes the actual value of the input Vector 3.
		public static void ClampAngle(ref Vector3 angle)
		{
			// If the input Vector 3's X value is less than -180, set that X value to its current value + 360. eg: if angle.x = -190, new value = 170
			if(angle.x < -180) angle.x += 360;
			// else if the input Vector 3's x value is greater than 180, set that X value to its current value + -360. eg: if anglex.x = 190, new value = -170.
			else if(angle.x > 180) angle.x -= 360;

			// does the same for the Y value
			if(angle.y < -180) angle.y += 360;
			else if(angle.y > 180) angle.y -= 360;

			// Does the same for Z value
			if(angle.z < -180) angle.z += 360;
			else if(angle.z > 180) angle.z -= 360;

			// So if I understand correctly, the purpose of this method is to keep any rotation values within the range of -180 to 0 and 0 to +180. eg: remove the complications presented by the fact that -180 = 360. Ok. Makes sense.
		}
	}
}
