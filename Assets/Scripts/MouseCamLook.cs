using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MouseCamLook : MonoBehaviour {

	Vector2 mouseLook; //keep track of how much movement cam has made
	Vector2 smoothV; //smooth down the movement of the mouse (kinda multiplying movement by a Time.deltaTime thing
	public float sensitivity = 5.0f; //how much you have to move the mouse 
	public float smoothing = 2.0f; //smooth amount

	//points back to our character
	GameObject character; 

	// Use this for initialization
	void Start () 
	{
		// 'character' is the camera's parent (i.e. this' parent, because, cam is the current GObj)
		character = this.transform.parent.gameObject; 
	}
	
	// Update is called once per frame
	void Update () 
	{	
		//in each update get the change of mouse movement (mouse delta md)	
		var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		//that changing mouse is then multiplied by sensitivity & smoothign values
		md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
		smoothV.x = Mathf.Lerp (smoothV.x, md.x, 1f / smoothing); // lerp is a linear interpretation of movement which moves smoothly between 2 points, rather than snapping
		smoothV.y = Mathf.Lerp (smoothV.y, md.y, 1f / smoothing);
		//global value that's constantly adding up movement then has smoothV added to it
		mouseLook += smoothV;
		mouseLook.y = Mathf.Clamp(mouseLook.y,-90f, 90f);

		//mouselook.y to rotate cam up/down around the right axis (x-axis)
		transform.localRotation = Quaternion.AngleAxis (-mouseLook.y, Vector3.right);
		//mouseLook.x value to rotate around the whole character's up, ot just the camera's
		character.transform.localRotation = Quaternion.AngleAxis (mouseLook.x, character.transform.up);

	}
}
