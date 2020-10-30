using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem; //get "hand" buttons from Interaction system

namespace CLE.OrbHunt
{
	public class DirectorControllerSymbolSelection : MonoBehaviour {

		public Hand controller1; // hand components of the interaction system for accessing buttons
		public GameObject symbolDisplay;
		public Sprite[] symbols;
		public bool communicationEnabled = false;
		[HideInInspector]public int x;

		// Update is called once per frame
		void Update () 
		{
			if (controller1.controller == null) return;
			if (RoundManager.clientRole == "Director") // only the director can send symbols (and therefore activate the symbol display on their controller)
			{
				if (!symbolDisplay.activeSelf && communicationEnabled && controller1.controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
				{
					InitiateSymbolSelector ();
				}
			}

			if (symbolDisplay.activeSelf)
			{
				// loop through symbols by pressing the grip
				if (Input.GetKeyDown ("s") || controller1.controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
					{
						if (x < 7)
						{
							x++;
						} 
						else
						{
							x = 0;
						}
						symbolDisplay.GetComponent<Image> ().sprite = symbols [x];
					}
				// send symbols with trigger
				else if (controller1.controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
					{
						SendSymbol (x);
					EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName, RoundManager.clientRole, "Pressed trigger to send "+symbols[x].name.ToString());
					}
			}
		}

		void InitiateSymbolSelector()
		{
			DisplaySymbolSelector (true);
			//ShuffleArray (symbols);// disabled for now [true shuffle would require to match symbol sprite for receiver by chckign for equal sprite .name rather than int index]
			x = Random.Range (0, 7);
			symbolDisplay.GetComponent<Image> ().sprite = symbols [x]; // show a random symbol first
		}


		public void DisplaySymbolSelector(bool display)
		{
			if (display)
			{
				symbolDisplay.SetActive(true);
			} 
			else if (display == false)
			{
				symbolDisplay.SetActive(false);
			}
		}

		public void SendSymbol(int symbolIndex)
		{
			Debug.Log (symbols[symbolIndex].name.ToString() + "sent");
			RoundManager.rm.SendSymbol (symbolIndex);
			DisplaySymbolSelector (false); // selector disappears
		}

		public void ShuffleArray<T>(T[] arr) 
		{
			for (int i = arr.Length - 1; i > 0; i--) 
			{
				int r = Random.Range(0, i + 1);
				T tmp = arr[i];
				arr[i] = arr[r];
				arr[r] = tmp;
			}
		}

	}
}