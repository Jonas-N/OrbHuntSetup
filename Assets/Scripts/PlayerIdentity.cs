// on the local player we're gonna get the unique nw ID (playerNetID), 
//turn it into a string (that can be used as a name)and give that to the server, 
//which will sync it to all the remote clients
//and then we'll have another fct to apply this identity to the GObj.s, so that each GObj. is unique
//this allows to assign both players a different role
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerIdentity : NetworkBehaviour {

	[SyncVar] public string playerUniqueName;
	private NetworkInstanceId playerNetID;
	private Transform myTransform;

	//participant role
	public enum participantRole {Seeker, Director, None}; // enumeration of type "participant role" that holds all possible values
	public participantRole role = participantRole.None;


	public override void OnStartLocalPlayer()
	{
		GetNetIdentity ();
		SetIdentity();
		GetComponent<MeshRenderer>().material.color = Color.cyan;
	}


	void start()
	{
		if (myTransform.name == "Player1") 
		{
			role = participantRole.Seeker;
		} 
		else 
		{
			role = participantRole.Director;
		}
	}

	// Use this for initialization
	void Awake () 
	{
		myTransform = transform;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(myTransform.name == "" || myTransform.name == "Player(Clone)") // if it is equal to empty string or Player(Clone), which is what is ritten automatically, then 
		{
			SetIdentity();
		}
		if (myTransform.name == "Player1") 
		{
			role = participantRole.Seeker;
		} 
		else 
		{
			role = participantRole.Director;
		}
	}

	[Client] //is never gonna run on the server anyway, but good to be sure that it can only be run on client
	void GetNetIdentity()
	{
		playerNetID = GetComponent<NetworkIdentity>().netId;
		CmdTellServerMyIdentity (MakeUniqueIdentity()); //need a function that creates the unique name to tell to the sevrer (below)
	}

	//name should be set on the server and not just the clients
	void SetIdentity()
	{
		if(!isLocalPlayer)
		{
			myTransform.name = playerUniqueName;
		}
		else
		{
			myTransform.name = MakeUniqueIdentity();
		}
	}

	string MakeUniqueIdentity ()
	{
		string uniqueName = "Player" + playerNetID.ToString();
		return uniqueName;
	}

	[Command]
	void CmdTellServerMyIdentity(string name)
	{
		playerUniqueName = name; // so that server can get the name, which is then synced
	}
		
}


