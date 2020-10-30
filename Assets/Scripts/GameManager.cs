using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
	[Tooltip("If not set, the player will default to the gameObject tagged as Player.")]
	public GameObject player;
	public static GameManager gm;
	public int score = 0; //initial score that gets updated when player's collect
//	[SyncVar(hook = )]private int syncScore;   ################
//	public float startTime=30; //initial time
	public Text mainScoreDisplay;
//	public Text mainTimerDisplay;
//	public bool gameIsOver = false;
//	public GameObject playAgainButtons; //currently not in editor
//	public string playAgainLevelToLoad; //currently unused
//	[SyncVar] private float currentTime;
//	private PlayerRole playerRole;


	// Use this for initialization
	void Start () 
	{	
		//set up score display
		Collect (0);
//		currentTime = startTime;

		// get a reference to the GameManager component for use by other scripts
		if (gm == null) 
			gm = this.gameObject.GetComponent<GameManager>();

		if (gm == null) 
			gm = gameObject.GetComponent<GameManager>();

		if (player == null) {
			player = GameObject.FindWithTag("Player");
		}

//		playerRole = player.GetComponent<PlayerRole>();

		//if (playerRole.role == "Seeker")
		// Orb visibility less	



		//////////////////////

		// init scoreboard to 0
		mainScoreDisplay.text = "0";

//		// inactivate the gameOverScoreOutline gameObject, if it is set
//		if (gameOverScoreOutline)
//			gameOverScoreOutline.SetActive (false);
//
//		// inactivate the playAgainButtons gameObject, if it is set
//		if (playAgainButtons)
//			playAgainButtons.SetActive (false);
//
//		// inactivate the nextLevelButtons gameObject, if it is set
//		if (nextLevelButtons)
//			nextLevelButtons.SetActive (false);


	}


	
	// Update is called once per frame
	void Update () 
		{
//		if (!gameIsOver) {
//			if (currentTime < 0) { // check to see if timer has run out
//				EndGame ();
//			} else { // game playing state, so update the timer
//				currentTime -= Time.deltaTime; // continue to reduce time based on the change of time between this frame and the prev. frame and then update main timer display
//				mainTimerDisplay.text = currentTime.ToString ("0.00");	//set the time to current time by formatting it via ToString to a general string pattern ""			
//			}
//		}
	}
	
//	void EndGame() 
//		{
//		// game is over
//		gameIsOver = true;
//
//			// repurpose the timer to display a message to the player
//		mainTimerDisplay.text = "GAME OVER";
//		}
	[Client] //collect orb on local client
	public void Collect(int amount)
	{
		score += amount;
		mainScoreDisplay.text = score.ToString ();
	}
		
//	[Command] //sync scores
//	void CmdSentCollectionToServer()
//	{
//		
//	}

}
