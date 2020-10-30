using System.Collections;
using System.Collections.Generic; //needed to use the list for smoothing
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[NetworkSettings(channel = 0, sendInterval = 0.1f)] // this way we can manually change the send interval of SyncPos
public class PlayerSyncPosition : NetworkBehaviour {

	[SyncVar (hook = "SyncPositionValues")] //send this value to all the clients **listcode: add the hook =..., --whenever server sends some data to the clients a function is also automatically run on the clients because this value has changed, so for the list we make a hook, fct will be defined below
	private Vector3 syncPos; //position that is to be synced
	[SerializeField] Transform playerTransform; //need to put the transform of the player in here in the editor
	private float lerpRate; //used to be = 15 and [SerializeField], before **listcode
	private float normalLerpRate = 16; // **listcode
	private float fasterLerpRate = 27; // **listcode

	private Vector3 lastPos;
	private float threshold = 0.5f; //when the player moves: for every half a meter the player mvoes send a command

	//getting the latency, for this we have to simulate a network in the game manager (with lag and paket loss, all ocde belonging to this is marked *latencycode
	private NetworkClient nClient; //*latencycode
	private int latency; //*latencycode
	private Text latencyText;//*latencycode, text box to show latency, requires UI namespace

	//movement list for smoothing of remote players' movements, **listcode
	private List<Vector3> syncPosList = new List<Vector3>(); //**listcode, list to store movements
	[SerializeField] private bool useHistoricalLerping = false; //**listcode
	private float closeEnough = 0.1f; //**listcode

	void Start()
	{
		nClient = NetworkManager.singleton.GetComponent<NetworkManager> ().client; //this is for showing the latency, to get access to the NW Manager, *latencycode
		latencyText = GameObject.Find("Latency Text").GetComponent<Text>(); //*latencycode
		lerpRate = normalLerpRate; // **listcode, just in case the lerpRate gets changed in the editor or otherwise
	}


	void Update()
	{
		LerpPosition (); //putting the lerp here, makes Time.deltaTime vary as the framerate changes | interpolation can now happen at same speed across diff. machines
						 //Time.deltaTime is the time between two frames and this values is constantly changing when used in Update() and so enables us to decouple from fps when implementing smooth interpolation
						 //in FixedUpdate Time.deltaTime becomes fixed time step and is no longer time between frames --> less good for interpolation because the interpolation speeed will differ between computers
		ShowLatency();   //*latencycode
	}


	void FixedUpdate() //needs FixedUpdate() rather than Update() to reduce the no. of times we wanna send msgs over the NW
	{
	// make use of the functions below, so that when fixed update runs in our game instance position is transmitted...
		//...by calling [Cmd] below, this command is run on server and server receives value (pos) on script attached to character in the server...
		// ... and this value will have changed and will tehrefore be sent to all the clients
		// so on other clients the LerpPosition() will run and they will see the position lerp
		TransmitPosition(); 
		//LerpPosition(); // lerp in FixedUpdate() was wrong, should be in Update, because for some machines Time.deltaTime might be different!!
	}
	//a function to lerp is needed
	void LerpPosition()
	{
		//lerping the position should only happen with player chars that don't belong to us
		//these players are then smoothly transferred to the incrementally incoming positions
		if(!isLocalPlayer)
		{
			if(useHistoricalLerping)//**listcode
			{
				HistoricalLerping ();
			}
			else//**listcode
			{
				OrdinaryLerping ();//** listCode, used to be simply what is now in OrdinaryLerping() in the if statement above (!isLocalPlayer)...
			}
		}
	}

	//need to tell server about this position and then all the game instances/clients
	//and then all clients will use this LerpPosition fct. to move the chatacter there

	[Command] //information that is sent to the server
	void CmdProvidePositionToServer(Vector3 pos) //needs to have Cmd in front
	{
		//this is sent to server
		syncPos = pos; //so on the server syncpos will equal the position (NOT on the clients)
		//server then has to sync this value  --> this is done with [SyncVar], because the server autoamtically sends SyncVar values to all clients when they change
//		Debug.Log("Command for position sent: " +pos);
	}

	//run a function only on the clients
	[ClientCallback] //even though one client is the server, it can still do the client callback as a client
	void TransmitPosition()
	{
		if (isLocalPlayer && Vector3.Distance(playerTransform.position, lastPos) > threshold) //everytime the local player has moved a distance between the two positions and this is higher than threshold, then we send the command, otherwise not
		{
			CmdProvidePositionToServer (playerTransform.position);
			lastPos = playerTransform.position; // lastPos needs to be captured
		}
	}

	[Client] //**listcode, fct. should only be run on clients
	void SyncPositionValues(Vector3 latestPos) //**listcode, fct. has input which is a vector3
	{
		syncPos = latestPos;//syncpos value on clients should be equal to latestPos
		syncPosList.Add(syncPos); //add an entry to the list (so the server gets latest syncpos via the Command, so syncPos is assigned that value on the server and then via SyncVar it is sent to the clients, now it knows that there is a hook fct (this one), and this is gonna be called on the clients and the input for this is that syncPos value, and then on the clients this variable will be set, and a value is added to the list, and then we lerp this list towards the items in the list
	}

	void ShowLatency()  //*latencycode (whole function)
	{
		if (isLocalPlayer) 
		{
			latency = nClient.GetRTT ();
			latencyText.text = latency.ToString();
		}
	}

	void OrdinaryLerping() //**listcode (whole fct)
	{
		playerTransform.position = Vector3.Lerp (playerTransform.position, syncPos, Time.deltaTime * lerpRate); // * lerpRate is to smooth out according to the number
	}

	void HistoricalLerping() //**listcode (whole fct)
	{
		if(syncPosList.Count > 0) //no point in lerping when the list is empty
		{
			playerTransform.position = Vector3.Lerp (playerTransform.position, syncPosList[0], Time.deltaTime * lerpRate); //interpolate into the 1st position in the list
			//when lerping to a position in this manner, you want to move to the next waypoint, so when being close enough to item in the list, it should be knocked out of the list --> for this we create the var closeEnough
			if(Vector3.Distance(playerTransform.position, syncPosList[0]) < closeEnough) //if distance between these two is less than closenough...
			{
				syncPosList.RemoveAt(0);//...then this entry will be removed from the list
				// --> as soon as this happens, the player will not try to move towards that value anymore
			}
			//the list got really big and the player moved to far into the past, let's do something up that
			if (syncPosList.Count > 10) {
				lerpRate = fasterLerpRate; // then make the lerpRate equal to a faster one, so that the player moves faster
			} else 
			{
				lerpRate = normalLerpRate; // a lot of trial and error is involved in getting suitable values for the lerRates to get a good result
			}
//			Debug.Log (syncPosList.Count.ToString ());
		}
	}
}

