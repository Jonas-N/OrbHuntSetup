using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
{
	public class RoundAndScoreSaver : MonoBehaviour {
		public static RoundAndScoreSaver saver;
		public int round = 0;
		public int score = 0;
				
		void Awake ()   
		{
			if (saver == null)
			{
				DontDestroyOnLoad(gameObject);
				saver = this;
			}
			else if (saver != this)
			{
				Destroy (gameObject);
			}
		}

		public void SaveRoundAndScore()
		{
			round = RoundManager.rm.round;
			score = EnvironmentGameManager.gm.score;
		}
	}
}