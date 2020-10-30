using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
	{
	public class CopyScript_NonVrAvatar : Photon.MonoBehaviour { //allows to use Photon View

		// Update is called once per frame
		void Update () 
		{
			if(photonView.isMine)
			{	
				transform.position = testingPPlayerBodyRef.Instance.capsuleBody.transform.position;
			}		
		}
	}
}