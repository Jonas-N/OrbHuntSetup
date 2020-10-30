using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameTimer : NetworkBehaviour {
	[SyncVar] public float gameTime; //The length of a game, in seconds.
	[SyncVar] public float timer; //How long the game has been running. -1=waiting for players, -2=game is done
	[SyncVar] public int minPlayers; //Number of players required for the game to start
	[SyncVar] public bool masterTimer = false; //Is this the master timer in the nw?
	public Text mainTimerDisplay;
	//public ServerTimer timerObj;

	GameTimer serverTimer; //create object serveTimer of type GameTimer

	void Start()
	{
		if(isServer) //if it's the server (host client)
		{ 
			// For the host to do: use the timer and control the time. (all clients get their time from the master timer)
			if (isLocalPlayer) 
			{
				serverTimer = this;
				masterTimer = true; //host becomes master Timer
			}
		}
		else if(isLocalPlayer) //if it's a client
		{ 
			//For all the normal clients to do: get the host's timer.
			GameTimer[] timers = FindObjectsOfType<GameTimer>(); //the way I understand it: for all timers that are serverTimer, if they are the master timer, server Timer's time is set equal to their value
			for(int i =0; i<timers.Length; i++)
			{
				if(timers[i].masterTimer)
				{
					serverTimer = timers[i];
				}
			}
		}
	}
	void Update()
	{
		if(masterTimer)
		{ //Only the MASTER timer controls the time
			if(timer>=gameTime)
			{
				timer = -2;
			}
			else if(timer == -1) //waiting for players
			{
				if(NetworkServer.connections.Count >= minPlayers) //check whether enough players connected
				{
					timer = 0;
				}
			}
			else if(timer == -2)
			{
				//Game done.
			}
			else
			{
				timer += Time.deltaTime;
				mainTimerDisplay.text = timer.ToString ("0.00");	//set the time to current time by formatting it via ToString to a general string pattern ""			

			}
		}

		if(isLocalPlayer)
		{ //EVERYBODY updates their own time accordingly.
			if (serverTimer) 
			{
				gameTime = serverTimer.gameTime;
				timer = serverTimer.timer;
				minPlayers = serverTimer.minPlayers;
			} 
			else { //Maybe we don't have it yet?
				GameTimer[] timers = FindObjectsOfType<GameTimer>();
				for(int i =0; i<timers.Length; i++)
				{
					if(timers[i].masterTimer)
					{
						serverTimer = timers [i];
					}
				}
			}
		}
	}
}