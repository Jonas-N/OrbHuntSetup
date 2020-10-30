using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CLE.OrbHunt
{
	/// Player name input field. Let the user input his name, will appear above the player in the game.
	[RequireComponent(typeof(InputField))] //automatically adds InputField componetn to GObj if not already there
	public class PairIdInputField : MonoBehaviour
	{
		#region Private Variables
//		static string pairIdPrefKey = "PairID";
		public static string pairID = "No pair ID set";
		#endregion


		#region MonoBehaviour CallBacks
		void Start () 
		{
			var input = gameObject.GetComponent<InputField>();
			input.onEndEdit.AddListener(SubmitPairID);

			// ----------------------------------------for some reason the old code relying on playerprefs doesn't work anymore (variable remains blank): it could have something to do with how the string is passed on from the input field in the efitor
//			string defaultName = "";
//			InputField _inputField = this.GetComponent<InputField>();
//			if (_inputField!=null)
//			{
//				if (PlayerPrefs.HasKey(pairIdPrefKey))
//				{
//					defaultName = PlayerPrefs.GetString(pairIdPrefKey);
//					_inputField.text = defaultName;
//				}
//			}
//			pairID =  defaultName;
			// ----------------------------------------
		}
		#endregion


		#region Public Methods
		// ----------------------------------------
		/// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
		/// <param name="value">The name of the Player</param>
//		public void SetPairID(string value)
//		{
//			pairID = value; 
//
//			PlayerPrefs.SetString (pairIdPrefKey, value);
//			DebugWrapper.Debug ("Pair ID is " + pairID.ToString ());
//		}
		// ----------------------------------------
		public void SubmitPairID(string enteredString)
		{
			DebugWrapper.Debug("Pair ID is: "+ enteredString);
			pairID = enteredString;
		}
		#endregion
	}
}