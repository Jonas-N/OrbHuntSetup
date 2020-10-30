using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeInstructionImage : MonoBehaviour {
	
	public Image instructionImageDisplay; // the image GameObj that is a child of the canvas

	// Use this for initialization
	void Start () 
	{
		
		//make sure that instruction images can be displayed by getting the image component of the display
		instructionImageDisplay = GetComponent<Image>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateInstructionImg(Sprite sprite)
	{
		instructionImageDisplay.sprite = sprite;
	}




}
