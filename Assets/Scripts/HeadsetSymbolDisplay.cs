using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CLE.OrbHunt
{
	public class HeadsetSymbolDisplay : MonoBehaviour 
	{

		public GameObject symbolDisplay;
		public Sprite[] symbols;

		public void DisplayReceivedSymbol(int i) // change the sprite to received symbol
		{
			symbolDisplay.GetComponent<Image>().enabled = true;
			symbolDisplay.GetComponent<Image> ().sprite = symbols [i];
			StartCoroutine("FadeSymbol");
			EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName, RoundManager.clientRole, "symbol displayed: "+symbols[i].name.ToString());
		}
			
		IEnumerator FadeSymbol() // show it for 2 seconds only
		{
			yield return new WaitForSeconds(2);
			symbolDisplay.GetComponent<Image>().enabled = false;
		}
	}
}
