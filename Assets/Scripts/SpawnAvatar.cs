using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
	{
	public class SpawnAvatar : MonoBehaviour {
		public GameObject headPrefab;
		public GameObject leftHandPrefab;
		public GameObject rightHandPrefab;
		// Use this for initialization
		void Start () 
		{
			PhotonNetwork.Instantiate(headPrefab.name, ViveManager.Instance.head.transform.position, ViveManager.Instance.head.transform.rotation, 0); // 0 is the group that has to be provided
			PhotonNetwork.Instantiate(leftHandPrefab.name, ViveManager.Instance.leftHand.transform.position, ViveManager.Instance.leftHand.transform.rotation, 0); // #3
			PhotonNetwork.Instantiate(rightHandPrefab.name, ViveManager.Instance.rightHand.transform.position, ViveManager.Instance.rightHand.transform.rotation, 0); // #3
//			Debug.Log("Avatar spawned");
		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}