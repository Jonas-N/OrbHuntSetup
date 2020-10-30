using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
{
	
	public class NwManager : MonoBehaviour {

	//	public GameObject avatarPrefab;
		public GameObject headPrefab;
		public GameObject leftHandPrefab; // could also have just one for hand (if using only one controller)
		public GameObject rightHandPrefab;

		public const string VERSION = "1.0";

		// Use this for initialization
		public void Start () 
		{
			PhotonNetwork.ConnectUsingSettings (VERSION);

		}

		// v--------- These methods are from the ConnectedAndJoinRandom.cs from Photon

		public virtual void OnConnectedToMaster() //connecting to that photon server
		{
			Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
			PhotonNetwork.JoinRandomRoom(); //we want to join a random room
		}

		public virtual void OnJoinedLobby()
		{
			Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
			PhotonNetwork.JoinRandomRoom();
		}

		//if there are no rooms to join it will fail
		public virtual void OnPhotonRandomJoinFailed() 
		{
			Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
			PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, null); // if we can't join a room, create one, in this case for 4 players
		}


		//we are then logging this

		public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
		{
			Debug.LogError("Cause: " + cause);
		}

		// when we actually join the room:
		public void OnJoinedRoom()
		{
			Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
	//		GameObject go = PhotonNetwork.Instantiate (avatarPrefab.name, Vector3.zero, Quaternion.identity, 0); // when joined room instantiate the avatar [photon asks for string name --> .name] on NW on a bunch of gameobjects,  #1
			PhotonNetwork.Instantiate(headPrefab.name, ViveManager.Instance.head.transform.position, ViveManager.Instance.head.transform.rotation, 0); // 0 is the group that has to be provided
			PhotonNetwork.Instantiate(leftHandPrefab.name, ViveManager.Instance.leftHand.transform.position, ViveManager.Instance.leftHand.transform.rotation, 0); // #3
			PhotonNetwork.Instantiate(rightHandPrefab.name, ViveManager.Instance.rightHand.transform.position, ViveManager.Instance.rightHand.transform.rotation, 0); // #3
		}

		// ^--------- These methods are from the ConnectedAndJoinRandom.cs from Photon
	}
}