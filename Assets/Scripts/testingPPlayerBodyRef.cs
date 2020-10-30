using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
	{
	public class testingPPlayerBodyRef : MonoBehaviour {

		public GameObject capsuleBody;

		public static testingPPlayerBodyRef Instance; //this is our singleton for controlling the Vive, so globally thruout the project we could go ViveManager.instance.head to access the head for example

		// get access to our head and left/right hand

		void Awake () 
		{
			if (Instance == null) 
			{
				Instance = this;
			}
		}
		
		void OnDestroy()
		{
			if (Instance == this) 
			{
				Instance = null;
			}
		}

	}
}