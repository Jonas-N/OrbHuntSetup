// attach this script to steamVR camera

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
{
	public class PlayerLookingDirectionLogger : MonoBehaviour {
	
//		// Alternative solution: get looking direction for every frame
//		void Update ()
//		{
//			if (RoundManager.rm.round > 0 && RoundManager.rm.roundRunning)
//			{
//				EnvironmentGameManager.gm.LogLookingDirectionInCSV (PhotonNetwork.playerName, RoundManager.clientRole, transform.forward);
//			}
//		}

		void Start()
		{
			InvokeRepeating("LogLookingDirection", 0.0f, 0.1f);
		}


		public void LogLookingDirection()
		{
			if (RoundManager.rm.round != 0 && RoundManager.rm.roundRunning)
			{
				EnvironmentGameManager.gm.LogLookingDirectionInCSV (PhotonNetwork.playerName, RoundManager.clientRole, transform.forward);
			}
		}




	}
}

