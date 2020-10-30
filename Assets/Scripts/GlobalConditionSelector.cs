using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
{
	public class GlobalConditionSelector : MonoBehaviour {
		public static GlobalConditionSelector csInstance;
		public int condition = 0;
		public int block = 0; // holds experimental block number
		// Use this for initialization
		void Start () {
			DebugWrapper.Debug ("Selected condition = "+ condition.ToString());
		}

		void Awake ()   
		{
			if (csInstance == null)
			{
				DontDestroyOnLoad(gameObject);
				csInstance = this;
			}
			else if (csInstance != this)
			{
				Destroy (gameObject);
			}
		}
		// Update is called once per frame
		void Update () 
		{
			if (block == 0 && Input.GetKeyDown ("l")) 
			{
				LoadCondition ();
			}	
		}

		public void LoadCondition()
		{
			PhotonView photonView = PhotonView.Get (this);
			photonView.RPC ("MoveToNextBlock", PhotonTargets.All);
			if (condition == 1)
			{
				PhotonNetwork.LoadLevel ("Slope");

			} 
			else if (condition == 2)
			{
				PhotonNetwork.LoadLevel ("Forest");
			} 
			else
			{
				DebugWrapper.Debug ("Error loading condition");
			}
		}

		[PunRPC]
		void MoveToNextBlock()
		{
			block++; // we move to the next experimental block
		}
	}
}