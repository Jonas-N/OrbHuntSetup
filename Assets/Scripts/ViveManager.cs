using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
	{
	public class ViveManager : MonoBehaviour {

		public GameObject head;
		public GameObject leftHand;
		public GameObject rightHand;

		public static ViveManager Instance; //this is our singleton for controlling the Vive, so globally throughout the project we could go ViveManager.instance.head to access the head for example

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