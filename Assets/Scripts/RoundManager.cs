// this script is for handling rounds (trials) and time, other global methods, like the collection of orbs are handled in the EnvironmentGameManager script (timer inspired by https://github.com/googlearchive/tango-examples-unity/blob/master/TangoWithMultiplayer/Assets/Photon%20Unity%20Networking/UtilityScripts/InRoomRoundTimer.cs)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon; //for photon hashtable that is needed to store the time:
using Valve.VR.InteractionSystem; //get "hand" buttons from Interaction system

namespace CLE.OrbHunt
{
	public class RoundManager : Photon.MonoBehaviour
	{
		#region Variables
		[Header( "Player and Avatar" )]
		public static RoundManager rm = null;
		public static GameObject LocalPlayerGameObject = null; // the local player prefab
		[HideInInspector] public float yOffsetLocalPlayer = 0.07f;
		public Hand controller1; // hand components of the interaction system for accessing buttons
		public Hand controller2;
		public GameObject seekerHeadPrefab;
		public GameObject directorHeadPrefab;
		public GameObject leftHandPrefab;
		public GameObject rightHandPrefab;
		public GameObject NonVRAvatarPrefab; // Capsule avatar for non-VR testing
		GameObject[] sceneGameObjects; // array that is filled with GObjs in the scene in the CleanPlayer() method
		public GameObject[] seekerSpawnPoints;
		public static string clientRole = "None"; // the client player's role
		public CameraOrbVisibility cameraScript; // drag camera that CameraOrbVisbility is attached to in here in the editor

		[Header( "Orb" )]
		[HideInInspector] public float yOffsetOrb = 0.5f; // orb should be slightly above ground
		public GameObject orbPrefab; // orb Prefab
		public GameObject[] orbSpawnPoints; // orb spawn location
		public static GameObject orbInstance = null; //the actual orb prefab instance that is spawned; a public static so that OrbBehaviour can access it via RoundManager.orbInstance
		[HideInInspector] public bool noOrbInScene = false;
		[HideInInspector] public int orbIndex =0; //# of orb per round

		[Header( "Time Management" )]
		public int numberOfRounds;
		[HideInInspector] public int round = 0; //the current round
		private const string StartTimeKey = "st"; //key that has the value of start time
		private double startTime; //this is the time from when a round has been started, it is defined below in OnPhotonCustomRoomPropertiesChanged
		private double elapsedTime;
		private double remainingTime;
		private bool firstRoundShouldStart = false;
		[HideInInspector] public bool roundRunning = false; //is set to true once the very first round is started after both players connect
		[HideInInspector] public bool confirmEnvironmentSwitch = false; // true once players can confirm switching to the next environment
		[Tooltip("Set length of each round")] public int roundTime;
		[HideInInspector] public double currentRoundTime; // this is for the csv log (the time in a round where an event was logged)

		[Header( "UI Displays" )]
		public Text timeDisplay;
		public Text roundDisplay;
		public Text roleDisplay;
		public Text controllerTimeDisplay;
		public Text controllerRoundDisplay;
		public Text controllerRoleDisplay;
		//confirmation screen
		public Text nextRoleMessage;
		public Text nextRoleMessageVR;
		private bool waitingForConfirmation = false;
		private bool seekerConfirmed = false;
		private bool directorConfirmed = false;
		//communication channel
		public DirectorControllerSymbolSelection channelScript;
		public HeadsetSymbolDisplay symbolDisplayScript;
		#endregion

		#region MonoBehaviour CallBacks
		void Awake()
		{
			if (rm == null)
			{
				rm = this; //when instance of RM is null, make this gameobject the RM
			}				
			else if (rm != this) //if there is another instance of GM destroy it
			{
				Destroy (gameObject);
			}
		}

		void Start () 
		{			
			if (PhotonNetwork.isMasterClient)
			{
				DebugWrapper.Debug ("This computer is the MasterClient");
			} 
			else if (!PhotonNetwork.isMasterClient)
			{
				DebugWrapper.Debug ("This computer is connected to the MasterClient");
			}
			PhotonNetwork.sendRate = 20;
			PhotonNetwork.sendRateOnSerialize = 10;

//			// We check which block we are in and make sure round and score are updated accordingly
			if (GlobalConditionSelector.csInstance != null)
			{
				if (GlobalConditionSelector.csInstance.block == 2)
				{
					EnvironmentGameManager.gm.score = RoundAndScoreSaver.saver.score;
					round = RoundAndScoreSaver.saver.round;
					// show correct round and score
					roundDisplay.text = round.ToString ();
					controllerRoundDisplay.text = round.ToString ();
					EnvironmentGameManager.gm.scoreDisplay.text = EnvironmentGameManager.gm.score.ToString ();
					EnvironmentGameManager.gm.controllerScoreDisplay.text = EnvironmentGameManager.gm.score.ToString ();
					if (PhotonNetwork.isMasterClient)
					{
						PhotonView photonView = PhotonView.Get (this);
						photonView.RPC ("WaitAndDisplayNextRole", PhotonTargets.All);
					}
				}
			}

			// Get Player
			if (LocalPlayerGameObject == null)
			{
				LocalPlayerGameObject = GameObject.FindGameObjectWithTag("Player");
				DebugWrapper.Debug ("Player in scene: " + LocalPlayerGameObject.name.ToString ());
			}


		}

		void Update ()
		{
			if (roundRunning == true)
			{
				elapsedTime = (PhotonNetwork.time - startTime); //this time is not displayed to the players, the difference between the time on network
				remainingTime = roundTime - (elapsedTime % roundTime);
				currentRoundTime = roundTime - remainingTime; //only for csv log timestamps
			}

			if (Input.GetKeyDown("space")) // MasterClient can start game by pressing space
			{
				//start round 1
//				elapsedTime = 0; //reset any elapsed time, so that round 1 can start normally and doesn't go directly into round 2
				if (PhotonNetwork.isMasterClient)// && round != switchRound)
				{
					PhotonView photonView = PhotonView.Get (this);
					photonView.RPC ("LoadRound", PhotonTargets.AllViaServer, Random.Range (0, 3));
					DebugWrapper.Debug ("MC loads first round");
					EnvironmentGameManager.gm.LogEventInCSV ("MasterClient", "no role", "first round started");
				} 
				DebugWrapper.Debug ("Space pressed. 1st round started");
			}

			if(Input.GetKeyDown("c"))
			{
				SwitchEnvironments();
			}

			if (controller1.controller == null) return;
			if (controller2.controller == null) return;
			if (waitingForConfirmation)
			{
				if ( Input.GetKeyDown ("x") || controller1.controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) || controller2.controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) )
				{
					waitingForConfirmation = false;
					DebugWrapper.Debug (clientRole+" confirmed");
					PhotonView photonView = PhotonView.Get (this);
					if (clientRole == "Seeker")
					{
						photonView.RPC ("ConfirmAsSeeker", PhotonTargets.All);
						EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName,clientRole,"confirmed");
					}
					else if (clientRole == "Director")
					{
						photonView.RPC ("ConfirmAsDirector", PhotonTargets.All);
						EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName,clientRole,"confirmed");
					}
				}
			}

			if (seekerConfirmed && directorConfirmed) // when both players have confirmed a new round starts
			{
				Debug.Log ("both confirmed");
				waitingForConfirmation = false;
				elapsedTime = 0;
				if (PhotonNetwork.isMasterClient && confirmEnvironmentSwitch == false)
				{
					PhotonView photonView = PhotonView.Get (this);
					photonView.RPC ("LoadRound", PhotonTargets.All, Random.Range (0, 3));
					DebugWrapper.Debug("Both players confirmed. Round " + round.ToString() + " starts.");
				} 
				else if (PhotonNetwork.isMasterClient && confirmEnvironmentSwitch == true)
				{
					confirmEnvironmentSwitch = false;
					SwitchEnvironments();
					DebugWrapper.Debug("Both players confirmed. Environment switched.");
				}

			} 

			// displaying time when games are running (note that round number is displayed via LoadRound() )
			if (roundRunning == true)
			{
				//we also need to make sure that new orbs are spawned whenever there is none in the scene
				if (noOrbInScene)
				{
					//					SpawnOrb();
					SpawnOrbRandomCircle (LocalPlayerGameObject.transform.position);
					noOrbInScene = false;
				}
					
				if (round <= numberOfRounds)
				{
					timeDisplay.color = Color.white;
					timeDisplay.text = remainingTime.ToString ("0.00");
					controllerTimeDisplay.color = Color.white;
					controllerTimeDisplay.text = remainingTime.ToString ("0.00");

					if (remainingTime < 10)
					{
						timeDisplay.color = Color.red;
						timeDisplay.fontStyle = FontStyle.Bold;
						controllerTimeDisplay.color = Color.red;
						controllerTimeDisplay.fontStyle = FontStyle.Bold;
					}
					else if (remainingTime < 30)
					{
						timeDisplay.color = Color.yellow;
						controllerTimeDisplay.color = Color.yellow;
					}

				}
				//every time we reach the end of a round a new round is loaded (for the number of rounds we set)
				if (elapsedTime > roundTime)
				{	
					if (round < numberOfRounds && round != numberOfRounds/2) //until the round before the last round, new rounds should be loaded whenever the timer goes down
					{
						if (PhotonNetwork.isMasterClient)
						{
//							DebugWrapper.Debug ("trying to start next round");
							PhotonView photonView = PhotonView.Get (this);
							photonView.RPC ("WaitAndDisplayNextRole", PhotonTargets.All);
						}
					}
					else if (round == numberOfRounds/2 && GlobalConditionSelector.csInstance.block == 1)
					{
						PhotonView photonView = PhotonView.Get (this);
						photonView.RPC ("EnvironmentSwitchInfo", PhotonTargets.All);
					}
					else if (round == numberOfRounds && GlobalConditionSelector.csInstance.block == 2)
					{
						if (PhotonNetwork.isMasterClient)
						{
							PhotonView photonView = PhotonView.Get (this);
							photonView.RPC ("FinishGameRPC", PhotonTargets.All);
						}
					}
				}
			}
		}
		#endregion


		#region Methods

		void PlacePlayerAvatar(GameObject randomSpawnPoint)
		{
			if (clientRole == "Seeker")
			{	
				// move player
				LocalPlayerGameObject.transform.position = randomSpawnPoint.transform.position;
				var rotationVector = LocalPlayerGameObject.transform.rotation.eulerAngles;
				rotationVector.x = 0;
				rotationVector.z = 0;
				LocalPlayerGameObject.transform.rotation = Quaternion.Euler(rotationVector); //make sure that player is not spawned tilted
				DebugWrapper.Debug ("Seeker spawned at "+ randomSpawnPoint.name.ToString());
				// spawn avatar
				PhotonNetwork.Instantiate(seekerHeadPrefab.name, ViveManager.Instance.head.transform.position, ViveManager.Instance.head.transform.rotation, 0); // 0 is the group that has to be provided
				PhotonNetwork.Instantiate(leftHandPrefab.name, ViveManager.Instance.leftHand.transform.position, ViveManager.Instance.leftHand.transform.rotation, 0);
				PhotonNetwork.Instantiate(rightHandPrefab.name, ViveManager.Instance.rightHand.transform.position, ViveManager.Instance.rightHand.transform.rotation, 0);
				EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName,clientRole,"spawned");
			}
			else if (clientRole == "Director") // place the director in a random circle around the seeker (distance is set in EnvironmentGameManager
			{
				// move player
				Vector3 selectedDirectorSpawnPoint=DefineDirectorRandomSpawnPoint(randomSpawnPoint);
				LocalPlayerGameObject.transform.position = selectedDirectorSpawnPoint;
				LocalPlayerGameObject.transform.LookAt(randomSpawnPoint.transform);
				var rotationVector = LocalPlayerGameObject.transform.rotation.eulerAngles;
				rotationVector.x = 0;
				rotationVector.z = 0;
				LocalPlayerGameObject.transform.rotation = Quaternion.Euler(rotationVector); //make sure that player is not spawned tilted
				PhotonNetwork.Instantiate(directorHeadPrefab.name, ViveManager.Instance.head.transform.position, ViveManager.Instance.head.transform.rotation, 0); // 0 is the group that has to be provided
				PhotonNetwork.Instantiate(leftHandPrefab.name, ViveManager.Instance.leftHand.transform.position, ViveManager.Instance.leftHand.transform.rotation, 0);
				PhotonNetwork.Instantiate(rightHandPrefab.name, ViveManager.Instance.rightHand.transform.position, ViveManager.Instance.rightHand.transform.rotation, 0);
				EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName,clientRole,"spawned");
				DebugWrapper.Debug ("Director spawned near Seeker");
			}
			else
			{
				DebugWrapper.Debug("Error: client role has not been assigned! Can't spawn player!");
			}
		}
		

		Vector3 DefineDirectorRandomSpawnPoint(GameObject chosenSeekerPosition)
		{
			Vector2 offset = Random.insideUnitCircle * EnvironmentGameManager.gm.directorSpawnPointDistance;
			Vector2 directorPosition = new Vector2 (chosenSeekerPosition.transform.position.x, chosenSeekerPosition.transform.position.z) + offset;
			float yVal = Terrain.activeTerrain.SampleHeight(new Vector3(directorPosition.x, 0, directorPosition.y)) + Terrain.activeTerrain.GetPosition().y; ;
			yVal = yVal + yOffsetLocalPlayer;
			Vector3 chosenDirectorPosition = new Vector3 (directorPosition.x, yVal, directorPosition.y);
			return chosenDirectorPosition;
		}

		Vector3 DefineDirectorRandomSpawnPoint2(GameObject chosenSeekerPosition)
		{
			Vector2 offset = Random.insideUnitCircle * EnvironmentGameManager.gm.directorSpawnPointDistance;
			Vector2 directorPosition = new Vector2 (chosenSeekerPosition.transform.position.x, chosenSeekerPosition.transform.position.z) + offset;
			Vector3 chosenDirectorPosition = new Vector3 (directorPosition.x, chosenSeekerPosition.transform.position.y, directorPosition.y);
			return chosenDirectorPosition;
		}

		void SpawnOrbRandomCircle(Vector3 currentPlayerPosition)
		{
			if (clientRole == "Seeker")
			{
				Vector3 selectedOrbSpawnPoint=DefineOrbRandomSpawnPoint(currentPlayerPosition);
				orbInstance = PhotonNetwork.Instantiate (orbPrefab.name, selectedOrbSpawnPoint, Quaternion.identity, 0);
				orbIndex++;
				EnvironmentGameManager.gm.LogEventInCSV ("orb","none","spawned");
			}
		}

		Vector3 DefineOrbRandomSpawnPoint(Vector3 currentPlayerPosition)
		{
			Vector2 offset = Random.insideUnitCircle * EnvironmentGameManager.gm.orbSpawnDistance;
			Vector2 orbPosition = new Vector2 (currentPlayerPosition.x, currentPlayerPosition.z) + offset;
			float yVal = Terrain.activeTerrain.SampleHeight(new Vector3(orbPosition.x, 0, orbPosition.y))+ Terrain.activeTerrain.GetPosition().y;
			yVal = yVal + yOffsetOrb;
			Vector3 chosenOrbPosition = new Vector3 (orbPosition.x, yVal, orbPosition.y); // Orb should be 0.55 above ground
//			DebugWrapper.Debug (round.ToString()+": Chosen Orb position:"+chosenOrbPosition.ToString());
			return chosenOrbPosition;
		}

		void CleanSpawnedAvatars()
		{
			// also Destroy all avatars
			sceneGameObjects = GameObject.FindGameObjectsWithTag ("Avatar");

			if (sceneGameObjects.Length != 0)
			{
				for(var i = 0 ; i < sceneGameObjects.Length ; i ++)
				{
					if(sceneGameObjects[i].GetComponent<PhotonView>().isMine == true && PhotonNetwork.connected == true)
					{
						PhotonNetwork.Destroy(sceneGameObjects[i]);
						DebugWrapper.Debug (sceneGameObjects [i].name + " removed");
					}
				}
			}
		}

		void CleanOrbs()
		{
			if (PhotonNetwork.isMasterClient)
			{	
				if (orbInstance != null)
				{
					PhotonNetwork.Destroy (orbInstance);
					DebugWrapper.Debug ("Orb cleaned");
				} 
				else if (orbInstance == null)
				{
					DebugWrapper.Debug ("Can't clean orb because orbInstance is null");	
				}
			}
		}

		[PunRPC]
		void LoadRound(int randomSpawnpoint)
		{
			DebugWrapper.Debug ("Trying to start next round");
			firstRoundShouldStart = false; // once the first round is started, it does not need to start again
			waitingForConfirmation = false;
			seekerConfirmed = false; // next round should require confirmation again
			directorConfirmed = false;
			nextRoleMessage.text = "";
			nextRoleMessageVR.text = "";
			roundRunning = true;
			channelScript.communicationEnabled = true; // allow director to send symbols
			CleanSpawnedAvatars ();
			round++;
			orbIndex = 0;
			roundDisplay.text = round.ToString ();
			controllerRoundDisplay.text = round.ToString ();
			DebugWrapper.Debug ("Round " + round + " started");
			//get the role of the player gameobject in the scene
			if(photonView.isMine) //[[this could be problematic]] for only getting the role of the player instantiated by the respective client
			{
				DebugWrapper.Debug ("Last role: " + clientRole); //log the last role the player had (initially None)
			}
			AssignClientPlayerRoles ();
			//once the role is clear we spawn the player in the right position
			GameObject pointToSpawn = seekerSpawnPoints [randomSpawnpoint]; // index the spawnpoint that was randomly selected in the LoadRound RPC
			PlacePlayerAvatar (pointToSpawn);
			SpawnOrbRandomCircle (LocalPlayerGameObject.transform.position);
			//access the current network time
			ExitGames.Client.Photon.Hashtable startTimeProp = new ExitGames.Client.Photon.Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
			startTimeProp[StartTimeKey] = PhotonNetwork.time;
			PhotonNetwork.room.SetCustomProperties(startTimeProp);              // implement OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged) to get this change everywhere
		}

		[PunRPC] //Inform pariticipants which role they will be in next round
		void WaitAndDisplayNextRole()
		{
			DebugWrapper.Debug ("Game is paused, role: "+clientRole);
			roundRunning = false;
			channelScript.communicationEnabled = false; //communication is halted, the seeker cannot open the symbol selector anymore
			channelScript.DisplaySymbolSelector(false);
			waitingForConfirmation = true; 
			if (clientRole == "Seeker")
			{
				nextRoleMessage.text = "Next round you will be\nthe <b><color=#3C62BFFF>Director</color></b>\n<size=40>Pull Trigger\nto continue.</size>";
				nextRoleMessageVR.text = "Next round you will be\nthe <b><color=#3C62BFFF>Director</color></b>\n<size=40>Pull Trigger\nto continue.</size>";
			} 
			else if (clientRole == "Director")
			{
				nextRoleMessage.text = "Next round you will be\nthe <b><color=#FFD800FF>Seeker</color></b>\n<size=40>Pull Trigger\nto continue.</size>";
				nextRoleMessageVR.text = "Next round you will be\nthe <b><color=#FFD800FF>Seeker</color></b>\n<size=40>Pull Trigger\nto continue.</size>";
			}
			if (orbInstance != null)
			{
					PhotonNetwork.Destroy (orbInstance);
					DebugWrapper.Debug ("Orb cleaned");
			}
//			ExitGames.Client.Photon.Hashtable startTimeProp = new ExitGames.Client.Photon.Hashtable();
//			startTimeProp[StartTimeKey] = PhotonNetwork.time;
//			PhotonNetwork.room.SetCustomProperties(startTimeProp);  
		}

		[PunRPC] //Inform pariticipants which role they will be in next round
		void EnvironmentSwitchInfo()
		{
			DebugWrapper.Debug ("Game is paused, role: "+clientRole);
			roundRunning = false;
			waitingForConfirmation = true;
			confirmEnvironmentSwitch = true;
			if (clientRole == "Seeker")
			{
				nextRoleMessage.text = "Next round you will\n<b><color=#bf3c48ff>switch environments</color></b>\nYou will be <b><color=#3C62BFFF>Director</color></b>\n<size=38>Pull Trigger to continue.</size>";
				nextRoleMessageVR.text = "Next round you will\n<b><color=#bf3c48ff>switch environments</color></b>\nYou will be <b><color=#3C62BFFF>Director</color></b>\n<size=38>Pull Trigger to continue.</size>";
			} 
			else if (clientRole == "Director")
			{
				nextRoleMessage.text = "Next round you will\n<b><color=#bf3c48ff>switch environments</color></b>\nYou will be <b><color=#FFD800FF>Seeker</color></b>\n<size=38>Pull Trigger to continue.</size>";
				nextRoleMessageVR.text = "Next round you will\n<b><color=#bf3c48ff>switch environments</color></b>\nYou will be <b><color=#FFD800FF>Seeker</color></b>\n<size=38>Pull Trigger to continue.</size>";
			}
			if (orbInstance != null)
			{
				PhotonNetwork.Destroy (orbInstance);
				DebugWrapper.Debug ("Orb cleaned");
			}
			//before switching environments, save round and score
			RoundAndScoreSaver.saver.SaveRoundAndScore();
		}


		[PunRPC]
		void ConfirmAsSeeker()
		{
			seekerConfirmed = true;
			DebugWrapper.Debug ("seeker confirm rpc");
		}

		[PunRPC]
		void ConfirmAsDirector()
		{
			directorConfirmed = true;
			DebugWrapper.Debug ("director confirm rpc");
		}

		[PunRPC]
		void FinishGameRPC()
		{
			timeDisplay.text = "Finished";
			timeDisplay.color = Color.green;
			timeDisplay.fontStyle = FontStyle.Italic;
			roundDisplay.text = numberOfRounds.ToString (); // does this really display the last round?
			controllerTimeDisplay.text = "Finished";
			controllerTimeDisplay.color = Color.green;
			controllerTimeDisplay.fontStyle = FontStyle.Italic;
			controllerRoundDisplay.text = numberOfRounds.ToString ();
			nextRoleMessageVR.text = "The experiment is now <b>finished</b>.\nPlease remove your headset.";
			roundRunning = false;
			DebugWrapper.Debug ("Experiment has finished");
			EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName,clientRole,"Experiment finished");
		}

		[PunRPC]
		void ReceiveSymbol(int symbolIndex)
		{
			symbolDisplayScript.DisplayReceivedSymbol(symbolIndex); // display the received symbol in the headsets of both players
		}

		public void SendSymbol(int symbolIndex)
		{
				PhotonView photonView = PhotonView.Get (this);
				photonView.RPC ("ReceiveSymbol", PhotonTargets.AllViaServer, symbolIndex); //it then passes the selected symbol index to an RPC, so that the seeker can receive it
				//EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName,clientRole,"sent msg:");
		}

		/// <summary>Called by PUN when new properties for the room were set (by any client in the room).</summary>
		public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			if (propertiesThatChanged.ContainsKey(StartTimeKey))
			{
				startTime = (double)propertiesThatChanged[StartTimeKey]; //start time gets the current value from the hashtable
			}
		}

		void AssignClientPlayerRoles()
		{
			// the MasterClient starts out as Seeker
			if (clientRole == "None" & PhotonNetwork.isMasterClient)
			{
				clientRole = "Seeker";
				roleDisplay.text = "Seeker";
				controllerRoleDisplay.text = "Seeker";
				cameraScript.ApplyLayerMasks (); // update camera according to player's role
			}
			else if (clientRole == "None" & !PhotonNetwork.isMasterClient)
			{
				clientRole = "Director";
				roleDisplay.text = "Director";
				controllerRoleDisplay.text = "Director";
				cameraScript.ApplyLayerMasks ();
			}
			else if (clientRole == "Seeker")
			{
				clientRole = "Director";
				roleDisplay.text = "Director";
				controllerRoleDisplay.text = "Director";
				cameraScript.ApplyLayerMasks ();
			}
			else if (clientRole == "Director")
			{
				clientRole = "Seeker";
				roleDisplay.text = "Seeker";
				controllerRoleDisplay.text = "Seeker";
				cameraScript.ApplyLayerMasks ();
			}
			else
			{
				DebugWrapper.Debug ("Problem with assigning Participant Role");
			}

			DebugWrapper.Debug ("Defined role: " + clientRole);
			//			return clientRole;
		}

		void SwitchEnvironments()
		{
			// select the condition that we are not in
			if (GlobalConditionSelector.csInstance.condition == 1)
				GlobalConditionSelector.csInstance.condition = 2;
			else if (GlobalConditionSelector.csInstance.condition == 2)
				GlobalConditionSelector.csInstance.condition = 1;
			else
				DebugWrapper.Debug ("No condition defined");
			if (GlobalConditionSelector.csInstance.condition != 0)
				GlobalConditionSelector.csInstance.LoadCondition ();
			EnvironmentGameManager.gm.LogEventInCSV (PhotonNetwork.playerName,clientRole,"environment switched");
		}
		
		//when the other player connects, enable the first round to start on both clients via RPC
		public void OnPhotonPlayerConnected( PhotonPlayer other  )
		{
			PhotonView photonView = PhotonView.Get (this);
//			photonView.RPC ("EnableRoundToStart", PhotonTargets.AllBuffered);
			DebugWrapper.Debug ("Other client connected. The game can now start");
		}

		void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{

		}
		#endregion
	}
}