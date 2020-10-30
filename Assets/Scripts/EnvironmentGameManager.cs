using System;
using System.Collections;
using System.Collections.Generic; // for Lists
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using System.IO; // for TextWriter (saving in csv)
using Valve.VR.InteractionSystem;

namespace CLE.OrbHunt
{
	public class EnvironmentGameManager : Photon.PunBehaviour //instead of MonoBehav, so that the GM can listen to players connection and disconntection
	{ 
		#region public variables
		public static EnvironmentGameManager gm = null; //make GM a singleton, that is accessible to others (static = the variable will belong to the class itself rather than an instance of the class)
//		public GameObject orb; //for accessing the collect function
		public int score = 0; //initial score
		public Text scoreDisplay;
		public Text controllerScoreDisplay;
		public bool twoPlayersConnected = false;
		public PhotonView gmPhotonView;
		public float directorSpawnPointDistance = 5f;
		public float orbSpawnDistance = 25f;
		public static String csvName = ""; // this csv logs all events
		public static String csvLookName = ""; //this csv holds players' looking direction (separately, since it's much larger)
		[HideInInspector] public string environmentOrder = "";
		[HideInInspector] public string sceneName = ""; // name of the current scene for csv
		public Transform cameraLookingDirection;
		#endregion

		#region MonoBehaviour CallBacks

		void Awake()
		{
			if (gm == null)
				gm = this; //when instance of GM is null, make this gameobject the GM
			else if (gm != this) //if there is another instance of GM destroy it
			{
				Destroy (gameObject);
			}
////			DontDestroyOnLoad (gameObject); //gameManager should never be destroyed on load
			sceneName = SceneManager.GetActiveScene().name;
		}
		void Start()
		{
			PhotonNetwork.sendRate = 20;
			PhotonNetwork.sendRateOnSerialize = 10;
			gmPhotonView = gameObject.GetComponent<PhotonView> ();

			DebugWrapper.Debug ("Players in scene: " + PhotonNetwork.playerList.Length.ToString());
		
		// for some reason this didn't work in experiment 3: "condition" was blank in the csv files of one computer, so environmentOrder must have not been assigned
			if (GlobalConditionSelector.csInstance.condition == 1)
			{
				environmentOrder = "slope_forest";
			}
			else if(GlobalConditionSelector.csInstance.condition == 2)
			{
				environmentOrder = "forest_slope";
			}
			// add this to debug issue (l. 52)
			// else
			// {
			// 	Debug.Log("can't get condition from GlobalConditionSelector")
			// }


			if (GlobalConditionSelector.csInstance != null)
			{
				if (GlobalConditionSelector.csInstance.block == 1) // make sure that the csv only gets created once (otherwise the round from the first block are overwritten when the csv is created again in block 2
				{
					if (PhotonNetwork.playerList.Length == 2)
					{
						CreateDataCSVs ();
						DebugWrapper.Debug ("CSV created");
					}
					else
					{
						DebugWrapper.Debug ("Can't create CSV, because there are not two players in the scene");
					}
				}
			}
		}

		void Update()
		{
			if (Input.GetKeyDown ("q"))
			{
				LeaveRoom ();
				DebugWrapper.Debug("Exit environment");
			}
		}
		#endregion


		#region Public Methods

		void CreateDataCSVs()
		{
			var participantNames = new List<string>();
			foreach (PhotonPlayer p in PhotonNetwork.playerList)
			{
				participantNames.Add (p.NickName); // put all the player names that appear in the network into a list
			}
			string clientIndex;
			if (PhotonNetwork.isMasterClient)
			{
				clientIndex = "_1";
			} 
			else
			{
				clientIndex = "_2";				
			}

			csvName = PairIdInputField.pairID + "_" + participantNames[1].ToString () + "_" + participantNames[0].ToString () + clientIndex +".csv"; // filename = [names of players separated with underscore].csv
			csvLookName = PairIdInputField.pairID + "_" + participantNames[1].ToString () + "_" + participantNames[0].ToString () + clientIndex +"_lookdir"+".csv";

			if (!Directory.Exists ("C:/OrbHuntData/")) //  create folder to hold CSV data, if it doesn't exist 
			{
				Directory.CreateDirectory ("C:/OrbHuntData/");
			}
			File.WriteAllText("C:/OrbHuntData/"+csvName, "pair_id,timestamp_game,timestamp_network,condition,environment,score,round,current_round_time,orb_index,object_id,player_role,event_logged,x_pos,y_pos,z_pos,x_look,y_look,z_look\n" ); // File.WriteAllText is inherited from System.IO
			File.WriteAllText("C:/OrbHuntData/"+csvLookName, "pair_id,timestamp_game,timestamp_network,condition,environment,score,round,current_round_time,orb_index,object_id,player_role,event_logged,x_pos,y_pos,z_pos,x_look,y_look,z_look\n" );
		}	
//		public void LogEventInCSV (string objectID, string playerRole, string eventLogged)
//		{
//			string path = "C:/OrbHuntData/"+csvName;
//			using (StreamWriter writer = new StreamWriter (path, true))//Writer that writes to the file path
//			{
//				if (objectID == "orb")
//				{
//					writer.WriteLine 
//					(
//						PairIdInputField.pairID +
//						"," + Time.time.ToString () +
//						"," + PhotonNetwork.time.ToString () +
//						"," + sceneName +
//						"," + score.ToString () +
//						"," + RoundManager.rm.round.ToString () +
//						"," + RoundManager.rm.currentRoundTime.ToString () +
//						"," + RoundManager.rm.orbIndex.ToString () +
//						"," + objectID +
//						"," + playerRole +
//						"," + eventLogged +
//						"," + RoundManager.orbInstance.transform.position.x.ToString () +
//						"," + RoundManager.orbInstance.transform.position.y.ToString () +
//						"," + RoundManager.orbInstance.transform.position.z.ToString ()	+
//						"," + "none"+
//						"," + "none" +
//						"," + "none"
//
//					);
//					writer.Close ();
//				} 
//				else
//				{
//					Vector3 position = RoundManager.LocalPlayerGameObject.transform.position;
//					writer.WriteLine 
//					(
//						PairIdInputField.pairID +
//						"," + Time.time.ToString () +
//						"," + PhotonNetwork.time.ToString () +
//						"," + sceneName +
//						"," + score.ToString () +
//						"," + RoundManager.rm.round.ToString () +
//						"," + RoundManager.rm.currentRoundTime.ToString () +
//						"," + RoundManager.rm.orbIndex.ToString () +
//						"," + objectID +
//						"," + playerRole +
//						"," + eventLogged +
//						"," + position.x.ToString () +
//						"," + position.y.ToString () +
//						"," + position.z.ToString () +
//						"," + cameraLookingDirection.forward.x.ToString () +
//						"," + cameraLookingDirection.forward.y.ToString () +
//						"," + cameraLookingDirection.forward.z.ToString ()
//					);
//					writer.Close ();
//				}
//			}	
//			DebugWrapper.Debug ("Event logged: " +playerRole + " " + eventLogged);
//		}
		public void LogEventInCSV (string objectID, string playerRole, string eventLogged)
		{
			string path = "C:/OrbHuntData/"+csvName;
			using (StreamWriter writer = new StreamWriter (path, true))//Writer that writes to the file path
			{
				// get player positions and time
				Vector3 playerPos = RoundManager.LocalPlayerGameObject.transform.position;
				string timeGame = Time.time.ToString ();
				string timeNetwork = PhotonNetwork.time.ToString ();
				string currentRoundTime = RoundManager.rm.currentRoundTime.ToString ();

				if (objectID == "orb")
				{
					Vector3 orbPos = RoundManager.orbInstance.transform.position;
					writer.WriteLine 
					(
						PairIdInputField.pairID +
						"," + Time.time.ToString () +
						"," + PhotonNetwork.time.ToString () +
						"," + environmentOrder +
						"," + sceneName +
						"," + score.ToString () +
						"," + RoundManager.rm.round.ToString () +
						"," + RoundManager.rm.currentRoundTime.ToString () +
						"," + RoundManager.rm.orbIndex.ToString () +
						"," + objectID +
						"," + playerRole +
						"," + eventLogged +
						"," + orbPos.x.ToString () +
						"," + orbPos.y.ToString () +
						"," + orbPos.z.ToString ()	+
						"," + "none"+
						"," + "none" +
						"," + "none"

					);
					writer.Close ();
				} 
				else
				{
					// log player stats in one line...
					writer.WriteLine 
					(
						PairIdInputField.pairID +
						"," + timeGame +
						"," + timeNetwork +
						"," + environmentOrder +
						"," + sceneName +
						"," + score.ToString () +
						"," + RoundManager.rm.round.ToString () +
						"," + currentRoundTime +
						"," + RoundManager.rm.orbIndex.ToString () +
						"," + objectID +
						"," + playerRole +
						"," + eventLogged +
						"," + playerPos.x.ToString () +
						"," + playerPos.y.ToString () +
						"," + playerPos.z.ToString () +
						"," + cameraLookingDirection.forward.x.ToString () +
						"," + cameraLookingDirection.forward.y.ToString () +
						"," + cameraLookingDirection.forward.z.ToString ()
					);
					// ... and orb position on the following line for the same point in time [if there is an orb when this event is logged]
					if (RoundManager.orbInstance != null && eventLogged != "orb collected")
					{
						Vector3 orbPos = RoundManager.orbInstance.transform.position;
						writer.WriteLine 
						(
							PairIdInputField.pairID +
							"," + timeGame +
							"," + timeNetwork +
							"," + environmentOrder +
							"," + sceneName +
							"," + score.ToString () +
							"," + RoundManager.rm.round.ToString () +
							"," + currentRoundTime +
							"," + RoundManager.rm.orbIndex.ToString () +
							"," + "orb" +
							"," + "none" +
							"," + "orb position during event above" +
							"," + orbPos.x.ToString () +
							"," + orbPos.y.ToString () +
							"," + orbPos.z.ToString ()	+
							"," + "none" +
							"," + "none" +
							"," + "none"
						);
						writer.Close ();
					}
				}
			}	
			if (objectID != "orb")
			{
				DebugWrapper.Debug ("Event logged: " + playerRole + " " + eventLogged);
			}
		}

		public void LogLookingDirectionInCSV (string objectID, string playerRole, Vector3 forwardVector)
		{
			string path = "C:/OrbHuntData/"+csvLookName;
			using (StreamWriter writer = new StreamWriter (path, true))//Writer that writes to the file path
			{
				Vector3 position = RoundManager.LocalPlayerGameObject.transform.position;
				writer.WriteLine 
				(
					PairIdInputField.pairID +
					"," + Time.time.ToString () +
					"," + PhotonNetwork.time.ToString () +
					"," + environmentOrder +
					"," + sceneName +
					"," + score.ToString () +
					"," + RoundManager.rm.round.ToString () +
					"," + RoundManager.rm.currentRoundTime.ToString () +
					"," + RoundManager.rm.orbIndex.ToString () +
					"," + objectID +
					"," + playerRole +
					"," + "looking_direction" +
					"," + position.x.ToString () +
					"," + position.y.ToString () +
					"," + position.z.ToString () +
					"," + forwardVector.x.ToString () +
					"," + forwardVector.y.ToString () +
					"," + forwardVector.z.ToString ()	
				);
				writer.Close ();
			}
		}


		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		[PunRPC]
		public void Collect(int amount)
		{
			score += amount;
			scoreDisplay.text = score.ToString ();
			controllerScoreDisplay.text = score.ToString ();
			DebugWrapper.Debug ("orb collected by seeker");
			PhotonNetwork.Destroy(CLE.OrbHunt.RoundManager.orbInstance); //the orb instance that is network-istantiated via roundmanager is destroyed
			RoundManager.rm.noOrbInScene = true;
			LogEventInCSV (PhotonNetwork.playerName,RoundManager.clientRole,"orb collected");
		}
		#endregion

		#region Photon Messages

		/// <summary>
		/// Called when the local player left the room. We need to load the launcher scene.
		/// </summary>
		public override void OnLeftRoom()
		{
			SceneManager.LoadScene(0);  //if we are not in a room we need to show the Launcher scene, so we are going to listen to OnLeftRoom() Photon Callback and load the lobby scene Launcher, which is indexed 0 in the Build settings Scene's list
		}

		//needed for RPCs
		void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
		}

		//informing us when a player connects
		public override void OnPhotonPlayerConnected( PhotonPlayer other  )
		{
			DebugWrapper.Debug( "OnPhotonPlayerConnected() " + other.NickName ); // not seen if you're the player connecting
			twoPlayersConnected = true;

			if ( PhotonNetwork.isMasterClient ) 
			{
				DebugWrapper.Debug( "OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient ); // called before OnPhotonPlayerDisconnected
			}
//			CreateDataCSV();
		}

		//informing us when a player disconnects
		public override void OnPhotonPlayerDisconnected( PhotonPlayer other  )
		{
			DebugWrapper.Debug( "OnPhotonPlayerDisconnected() " + other.NickName ); // seen when other disconnects
			twoPlayersConnected = false;

			if ( PhotonNetwork.isMasterClient ) 
			{
				DebugWrapper.Debug( "OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient ); // called before OnPhotonPlayerDisconnected
			}
		}

		#endregion
	}
}