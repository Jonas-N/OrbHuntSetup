using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _lookdir : MonoBehaviour {

	public Transform vrCam;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
//      Debug.Log (vrCam.forward);
		Debug.Log(vrCam.forward);
//		Debug.Log (Vector3.forward);
//		Debug.Log (vrCam.forward.x);
//		Debug.Log (vrCam.forward.y);
//		Debug.Log (vrCam.forward.z);
	}
}
