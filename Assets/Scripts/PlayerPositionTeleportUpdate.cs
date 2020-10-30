using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
{
	public class PlayerPositionTeleportUpdate : MonoBehaviour 
	{
		public GameObject child; //the steamVR player prefab that is a child of this GObj
		// Need to make sure that parent transform is updated whenever teleported, so that it can be logged in CSV
		void Update () {
			this.transform.position = child.transform.position;
		}
	}
}