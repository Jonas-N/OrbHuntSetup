using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
	{
	public class CopyScript : Photon.MonoBehaviour { //allows to use Photon View

		public int index = 1; // #4 // this index is different on each prefav (avatar, RHand, LHand, so that the script tracks the correct body part)
		
		// Update is called once per frame
		void Update () {
			if(photonView.isMine)
			{	
				// <#4>
				switch (index) 
				{
					case 1: // head
					transform.position = ViveManager.Instance.head.transform.position;
					transform.rotation = ViveManager.Instance.head.transform.rotation;
						break;
					case 2: // left hand 
					transform.position = ViveManager.Instance.leftHand.transform.position;
					transform.rotation = ViveManager.Instance.leftHand.transform.rotation;
						break;
					case 3: // right hand
					transform.position = ViveManager.Instance.rightHand.transform.position;
					transform.rotation = ViveManager.Instance.rightHand.transform.rotation;
						break;
				}
				// </#4>

	//			transform.position = ViveManager.Instance.head.transform.position; // comment as of #4
	//			transform.rotation = ViveManager.Instance.head.transform.rotation; // comment as of #4
			}		
		}
	}
}