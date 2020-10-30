using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CLE.OrbHunt
{
	/// Player name input field. Let the user input his name, will appear above the player in the game.
	[RequireComponent(typeof(InputField))] //automatically adds InputField componetn to GObj if not already there
	public class ParticipantInputField : MonoBehaviour
	{
		#region Private Variables
//		// Store the PlayerPref Key to avoid typos
//		static string playerNamePrefKey = "PlayerName";
		public static string participantID = "No participant ID set";
		#endregion


		#region MonoBehaviour CallBacks
		void Start () {
			var input = gameObject.GetComponent<InputField>();
			input.onEndEdit.AddListener(SubmitParticipantID);

			// ----------------------------------------for some reason the old code relying on playerprefs doesn't work anymore (variable remains blank): it could have something to do with how the string is passed on from the input field in the efitor

//			//Note: PlayerPrefs is a simple lookup list of paired entries (keys which are strings & value), this list should be [static] as it doesn't change.  If the PlayerPrefs has a given key, we can get it and inject that value directly when we start the feature, in our case we fill up the InputField with this when we start up, and during editing, We set the PlayerPref Key with the current value of the InputField, and we are then sure it's been stored locally on the User device for later retrieval ( the next time the user will open this game).
//			string defaultName = "";
//			InputField _inputField = this.GetComponent<InputField>();
//			if (_inputField!=null)
//			{
//				if (PlayerPrefs.HasKey(playerNamePrefKey))
//				{
//					defaultName = PlayerPrefs.GetString(playerNamePrefKey);
//					_inputField.text = defaultName;
//				}
//			}
//			PhotonNetwork.playerName =  defaultName; //This is main point of this script, setting up the name of the player over the network. The script uses this in two places, once during Start() after having check if the name was stored in the PlayerPrefs, and inside the public method SetPlayerName(). Right now, nothing is calling this method, we need to bind the InputField OnValueChange() to call SetPlayerName() so that every time the user is editing the InputField, we record it. We could do this only when the user is pressing play, however this is a bit more involving script wise, so let's keep it simple for the sake of clarity. It also means that no matter what the user will do, the input will be remembered, which is often the desired behavior.

			// ----------------------------------------
		}
		#endregion


		#region Public Methods
		// ----------------------------------------

//		/// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
//		/// <param name="value">The name of the Player</param>
//		public void SetPlayerName(string value)
//		{
//			// #Important
//			PhotonNetwork.playerName = value + " "; // force a trailing space string in case value is an empty string, else playerName would not be updated. [Note: removed trailing space " ", because of the csv in EnvironmentGameManager
//
//			PlayerPrefs.SetString(playerNamePrefKey,value);
//			DebugWrapper.Debug ("Participant ID is " + PhotonNetwork.playerName.ToString ());
//		}

		// ----------------------------------------


		public void SubmitParticipantID(string enteredString)
		{
			DebugWrapper.Debug("Participant ID is: "+ enteredString);
			participantID = enteredString;
			PhotonNetwork.playerName = participantID;
		}


		#endregion
	}
}