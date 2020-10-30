/// <summary>
/// Note: historical interpolation does NOT work yet, so rotation of head don't sync -> check code by https://www.youtube.com/watch?v=hgIByCgvq48
/// </summary>

using System.Collections;
using System.Collections.Generic; //to use list
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSyncRotation : NetworkBehaviour {

	[SyncVar (hook = "OnPlayerRotationSynced")] private float syncPlayerRotation; //used to be Quaternion which is less efficient, and added hook for hstorical syncing
	[SyncVar (hook = "OnCamRotationSynced")] private float syncCamRotation; //formely Quaterion
	[SerializeField] private Transform playerTransform;
	[SerializeField] private Transform camTransform;
	[SerializeField] private float lerpRate = 20;

	private float lastPlayerRot; //formely Quaterion
	private float lastCamRot; //formely Quaterion
	private float threshold = 1; //1 degrees of rotation will be necessary before command is send to server

	private List<float>syncPlayerRotList = new List<float>();
	private List<float>syncCamRotList = new List<float>();
	private float closeEnough = 0.4f;
	[SerializeField] private bool useHistoricalInterpolation;

	// Use this for initialization
	void Start () {
		
	}

	void Update()
	{
		LerpRotations ();
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		TransmitRotations ();
	}

	void LerpRotations()
	{
		if(!isLocalPlayer)
		{
			if (useHistoricalInterpolation) 
			{
				HistoricalInterpolation ();
			} 
			else 
			{
				OrdinaryLerping ();
			}
			//next two lines are commented out for the more efficient rotation sync
//			playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate); 
//			camTransform.rotation=Quaternion.Lerp(camTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate); //from playerTransform's current rotation to supplied synced player rotation,
		}	
	}

	void HistoricalInterpolation()
	{
		//first something needs to be in the list
		if(syncPlayerRotList.Count > 0)
		{
			LerpPlayerRot (syncPlayerRotList [0]);
			// like in the position script: if I'm close enough tot the value inthe list, knock it out of the list
			if(Mathf.Abs(camTransform.localEulerAngles.x - syncPlayerRotList[0]) < closeEnough)
			{
				syncPlayerRotList.RemoveAt (0);
			}
			Debug.Log(syncPlayerRotList.Count.ToString() + " syncPlayerRotList Count");
		}

		if(syncCamRotList.Count > 0)
		{
			LerpCamRot (syncCamRotList [0]);
			// like in the position script: if I'm close enough tot the value inthe list, knock it out of the list
			if(Mathf.Abs(playerTransform.localEulerAngles.y - syncCamRotList[0]) < closeEnough)
			{
				syncCamRotList.RemoveAt (0);
			}
			Debug.Log(syncCamRotList.Count.ToString() + " syncCamRotList Count");
		}
	}

	//two new functions needed now that we pass on the floats, the lerping is performed
	//this fct will include 2 subfunctions, one for lerping and one for historical interpolation, that can then be both called
	void OrdinaryLerping ()
	{
		LerpPlayerRot(syncPlayerRotation);
		LerpCamRot (syncCamRotation);
	}

	void LerpPlayerRot(float rotAngle)
	{
		//need a vector3 that represents the player's rotation now, and then once this is constructed, player's rotation is lerped to that new rotation that we made up
		Vector3 playerNewRot = new Vector3(0,rotAngle,0); //only y has a value
		playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, Quaternion.Euler(playerNewRot), lerpRate * Time.deltaTime); // the lerp happens from the player's rotation to the degree of the new rotation (which is why the quaternion has to be transformed into a Euler angle)
	}

	void LerpCamRot (float rotAngle)
	{
		Vector3 camNewRot = new Vector3 (rotAngle, 0, 0);
		camTransform.localRotation = Quaternion.Lerp (camTransform.localRotation, Quaternion.Euler (camNewRot), lerpRate * Time.deltaTime); // we want this to rotate relative to the parent transform, which is why we have to set local rotation (NOT global rotation, which is just "rotation" as for the player)
	}


	//command to supply server with these values
//	[Command] //note command fucntion only runs on the local player, you can never get remote players to talk to the server via comman
//	void CmdProvideRotationsToServer(Quaternion playerRot, Quaternion camRot)
//	{
//		syncPlayerRotation = playerRot;
//		syncCamRotation = camRot;
//		Debug.Log ("Command for angle sent");
//	}
//
//	//now a function to call this command (both functions will be called in FixedUpdate above
//	[ClientCallback] //in tutorial he just used [Client]
//	void TransmitRotations()
//	{
//		if (isLocalPlayer) //if it is the local player
//		{
//			if (Quaternion.Angle (playerTransform.rotation, lastPlayerRot) > threshold || Quaternion.Angle (camTransform.rotation, lastCamRot) > threshold) //angle determines angle between two Quaternions (a, b), so if player has rotated more than 5 degrees on Y axis, then command is called on the server, or if camera has titled more than 5 degrees on the X Axis
//			{
//				CmdProvideRotationsToServer (playerTransform.rotation, camTransform.rotation); //should provide player transform rotation and cam transform rotation
//				lastPlayerRot = playerTransform.rotation;
//				lastCamRot = camTransform.rotation;
//			}
//		}
//	}

	[Command]
	void CmdProvidePlayerRotationsToServer(float playerRot, float camRot)
	{
		syncPlayerRotation = playerRot;
//		Debug.Log ("playerRot: " + playerRot);
		syncCamRotation = camRot;
//		Debug.Log ("camRot: " + camRot);
	}


	// this was the non-float version:

//	[Command]
//	void CmdProvidePlayerRotationsToServer(Quaternion playerRot)
//	{
//		syncPlayerRotation = playerRot;
//		//Debug.Log ("playerRot: " + playerRot);
//	}
//
//	[Command]
//	void CmdProvideCameraRotationsToServer(Quaternion camRot)
//	{
//		syncCamRotation = camRot;
//		//Debug.Log ("camRot: " + camRot);
//	}

	[Client]
	void TransmitRotations()
	{
		//so on the local player perform a check before then running the command	
		if (isLocalPlayer) 
		{
//			if (Quaternion.Angle (playerTransform.rotation, lastPlayerRot) > threshold) // can be replaced by next line now that we have the CheckIfBeyondThreshold() fct
			if(CheckIfBeyondThreshold(playerTransform.localEulerAngles.y, lastPlayerRot) || CheckIfBeyondThreshold(camTransform.localEulerAngles.x, lastCamRot)) //we have to use EulerAngles, because we're not dealing with Quaternions anymore (i.e. rotations) --> Euler Angles are the actual degrees! (while Qu.s are just for values
			{
//				lastPlayerRot = playerTransform.rotation; //old version
				lastPlayerRot = playerTransform.localEulerAngles.y;
//				lastCamRot = camTransform.rotation; // old
				lastCamRot = camTransform.localEulerAngles.x;
//				cmd is sent to server and server is told what the latest rotation values are, so that it can sync it across clients
//				CmdProvidePlayerRotationsToServer (playerTransform.rotation); // old version --> supplied values had to be changed now (floats have to be supplied now)
				CmdProvidePlayerRotationsToServer(lastPlayerRot,lastCamRot);
			}
		}
	}﻿

	//fct that subtracts the two floats and returns absolute value ("Betrag") so that we can check that it is below threshold, where it shouldn't be send to the server
	bool CheckIfBeyondThreshold (float rot1, float rot2)
	{
		if (Mathf.Abs(rot1 - rot2) > threshold)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	//functions for hooks
	[Client]
	void OnPlayerRotationSynced(float latestPlayerRot)
	{
		syncPlayerRotation = latestPlayerRot; //player only rotates on Y axis and this should be send to the server, so we are cutting off the other 3 values kept by the quaternion and just use the float
		syncPlayerRotList.Add(syncPlayerRotation);
	}

	[Client]
	void OnCamRotationSynced(float latestCamRot)
	{
		syncCamRotation = latestCamRot; 
		syncCamRotList.Add(syncCamRotation);
	}
}
