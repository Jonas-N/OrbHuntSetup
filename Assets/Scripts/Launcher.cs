﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// this script takes functionality from https://doc.photonengine.com/en-us/pun/current/tutorials/pun-basics-tutorial/lobby
namespace CLE.OrbHunt
{
	public class Launcher : Photon.PunBehaviour
	{
		#region Public Variables
		/// <summary>
		/// The PUN loglevel. 
		/// </summary>
		public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
		[Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
		public byte MaxPlayersPerRoom = 2;
		[Tooltip("The Ui Panel to let the user enter name, connect and play")]
		public GameObject controlPanel;
		[Tooltip("The UI Label to inform the user that the connection is in progress")]
		public GameObject progressLabel;
		[Tooltip("Dropdown to select condition/environment")]
		public Dropdown conditionDropDown; //for selecting the condition on launch
		List<string> conditionNames = new List<string> () {"Select Environment", "Environment 1", "Environment 2" };
		[Tooltip("Text that indicates selected environment and warning if none is selected")]
		public Text conditionSelectionWarning;
		public static int selectedCondition; //environment that has been selected from DropDown
		public Button startButton;
		#endregion


		#region Private Variables
		/// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
		string _gameVersion = "1";
		/// <summary>
		/// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
		/// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
		/// Typically this is used for the OnConnectedToMaster() callback.
		/// </summary>
		bool isConnecting;
		#endregion

		#region MonoBehaviour CallBacks
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		void Awake()
		{
			// #Critical
			// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
			PhotonNetwork.autoJoinLobby = false;
			// #Critical
			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			PhotonNetwork.automaticallySyncScene = true; //if we are not master, this is important
			// #NotImportant
			// Force LogLevel
			PhotonNetwork.logLevel = Loglevel;
		}
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start()
		{
			progressLabel.SetActive(false); //at the beginning the user should not see "connecting", but the fields to enter name and select environment
			controlPanel.SetActive(true);
			PopulateConditionList (); //so that the conditions can be selected from the control Panel
//			Connect(); //now Connect is called via UI button
		}
		#endregion

		#region Methods for Condition Dropdown
		void PopulateConditionList() // add condition names to dropdown
		{
			conditionDropDown.AddOptions (conditionNames); // make condition names from a list of strings the options of the dropdown
		}

		public void DropdownIndexChanged(int index)  // in the editor, the event handler (Launcher in this case) needs to be dragged into the DropDown Object for this to work
		{
			if (index == 0)
			{
				conditionSelectionWarning.text = conditionNames [index] + "!";
				conditionSelectionWarning.color = Color.red;
				selectedCondition = 0;
				startButton.interactable = false;
			} else
			{
				conditionSelectionWarning.text = conditionNames [index] +" selected";
				conditionSelectionWarning.color = Color.white;
				selectedCondition = index;
				startButton.interactable = true;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Start the connection process. ((Connect() is called on press of the "Play button"))
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect()
		{
			// keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
			isConnecting = true;
			progressLabel.SetActive(true);
			controlPanel.SetActive(false);

			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (PhotonNetwork.connected)
			{
				// #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
				PhotonNetwork.JoinRandomRoom();
			}
			else
			{
				// #Critical, we must first and foremost connect to Photon Online Server.
				PhotonNetwork.ConnectUsingSettings(_gameVersion);
			}
		}

//		public void ChooseLevelBasedOnSelection()   // original from before adding training
//		{
//			if (selectedCondition == 1)
//			{
//				PhotonNetwork.LoadLevel ("Environment 1");
//			}
//			else if (selectedCondition == 2)
//			{
//				PhotonNetwork.LoadLevel ("Environment 2");
//			}
//			else
//			{
//				Debug.Log ("Can't play without environment selected");
//			}
//		}

		public void ChooseLevelBasedOnSelection()
		{
			if (selectedCondition == 1)
			{
				GlobalConditionSelector.csInstance.condition = 1;
				PhotonNetwork.LoadLevel ("Training");
			}
			else if (selectedCondition == 2)
			{
				GlobalConditionSelector.csInstance.condition = 2;
				PhotonNetwork.LoadLevel ("Training");
			}
			else
			{
				DebugWrapper.Debug ("Can't play without environment selected");
			}
		}
		#endregion


		#region Photon.PunBehaviour CallBacks
		public override void OnConnectedToMaster()
		{
			DebugWrapper.Debug("DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN");
			// we don't want to do anything if we are not attempting to join a room. 
			// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case we don't want to do anything.
			if (isConnecting) 
			{
				// #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
				PhotonNetwork.JoinRandomRoom();		
			}
		}
		
		public override void OnDisconnectedFromPhoton()
		{
			progressLabel.SetActive(false);
			controlPanel.SetActive(true);
			Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");        
		}


		public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
		{
			DebugWrapper.Debug("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 2}, null);");

			// #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
		PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
		}

		public override void OnJoinedRoom()
		{
			DebugWrapper.Debug("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
			// #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.automaticallySyncScene to sync our instance scene.
			if (PhotonNetwork.room.PlayerCount == 1)
			{
				DebugWrapper.Debug("Selected environment loaded");
				// #Critical
				// Load the Room Level.
				ChooseLevelBasedOnSelection();
			}
		}
		#endregion


	}
}