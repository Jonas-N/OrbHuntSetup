using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
{
	public class CameraOrbVisibility : MonoBehaviour {

		public Camera cam;
		public float orbVisibleFromDistance;
		public LayerMask seekerMask; 	//should be set to see everythign but OrbHint layer
		public LayerMask directorMask; 	//should be set to everything
		float[] layerCullDistances = new float[32];

		public void ApplyLayerMasks()
		{
			if (CLE.OrbHunt.RoundManager.clientRole == "Seeker")
			{
				// layer 9 contains Orb, see https://docs.unity3d.com/ScriptReference/Camera-layerCullDistances.html for logic
				layerCullDistances [9] = orbVisibleFromDistance;
				cam.layerCullDistances = layerCullDistances;
				cam.cullingMask = seekerMask;
			} 
			else if (CLE.OrbHunt.RoundManager.clientRole == "Director")
			{
				layerCullDistances [9] = 0; // Seeker can see orb normally
				cam.layerCullDistances = layerCullDistances;
				cam.cullingMask = directorMask;
			} 
			else if (CLE.OrbHunt.RoundManager.clientRole == "None")
			{
				DebugWrapper.Debug ("Can't set orb visibility (distance) because client role is not defined");
			}
			DebugWrapper.Debug ("Visibility updated");
		}
	}
}