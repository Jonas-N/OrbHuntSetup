//============================================================================================
//
// Based on Valve's InteractableExample: Copyright (c) Valve Corporation, All rights reserved.
//
//============================================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent( typeof( Interactable ) )]
	public class InteractableOrb : MonoBehaviour
	{
		// Orb vars
		public int orbValue = 1;
		public GameObject directorHint;
		public GameObject explosionPrefab;
		public GameObject aura;
		// Interaction vars
		private TextMesh textMesh;
		private Vector3 oldPosition;
		private Quaternion oldRotation;
		private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & ( ~Hand.AttachmentFlags.SnapOnAttach ) & ( ~Hand.AttachmentFlags.DetachOthers );
		public bool immediateCollection = true; // allow seeker to collect orb by touching it

		//-------------------------------------------------
		void Awake()
		{
			textMesh = GetComponentInChildren<TextMesh>();
			textMesh.text = "";
		}

		void Start()
		{
			PhotonNetwork.sendRate = 20;
			PhotonNetwork.sendRateOnSerialize = 10;
		}

		//-------------------------------------------------
		// Called when a Hand starts hovering over this object
		//-------------------------------------------------
		private void OnHandHoverBegin( Hand hand )
		{
			if (immediateCollection == false)
			{
				if (CLE.OrbHunt.RoundManager.clientRole == "Seeker")
				{
					textMesh.text = "Orb found! \nGrab it with the grip button!";
					aura.gameObject.SetActive (false);
				} else if (CLE.OrbHunt.RoundManager.clientRole == "Director")
				{
					textMesh.text = "Only the Seeker can collect orbs!";
				}
			} 
			// collect immediately
			else if (CLE.OrbHunt.RoundManager.clientRole == "Seeker")
			{
				if (explosionPrefab != null)
				{
					PhotonNetwork.Instantiate (explosionPrefab.name, CLE.OrbHunt.RoundManager.orbInstance.transform.position, Quaternion.identity,0);
					CLE.OrbHunt.DebugWrapper.Debug ("orb is collected and disappears");
				}
				hand.HoverUnlock( GetComponent<Interactable>() );
				transform.position = new Vector3 (0,0,0);
				//hand.HoverUnlock (null);
				if (CLE.OrbHunt.EnvironmentGameManager.gm != null) //if there is a gm (object of the type GameManager)
				{
					// tell the environment game manager to Collect the Orb on all PCs
					CLE.OrbHunt.EnvironmentGameManager.gm.gmPhotonView.RPC ("Collect", PhotonTargets.AllBufferedViaServer, orbValue);
				}
			}
				
		}


		//-------------------------------------------------
		// Called when a Hand stops hovering over this object
		//-------------------------------------------------
		private void OnHandHoverEnd( Hand hand )
		{
			textMesh.text = "";
		}


		//-------------------------------------------------
		// Called every Update() while a Hand is hovering over this object
		//-------------------------------------------------
		private void HandHoverUpdate( Hand hand )
		{
			if (immediateCollection == true)
			{
				hand.HoverUnlock( GetComponent<Interactable>() );
			}
			if (CLE.OrbHunt.RoundManager.clientRole == "Seeker")
			{
//				if ( hand.GetStandardInteractionButtonDown() || ( ( hand.controller != null ) && hand.controller.GetTouch( Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger ) ) || ( ( hand.controller != null ) && hand.controller.GetPressDown( Valve.VR.EVRButtonId.k_EButton_Grip ) ))
				if ( ( hand.controller != null ) && hand.controller.GetPressDown( Valve.VR.EVRButtonId.k_EButton_Grip ) )
				{
					if ( hand.currentAttachedObject != gameObject )
					{
						// Save our position/rotation so that we can restore it when we detach
						oldPosition = transform.position;
						oldRotation = transform.rotation;

						// Call this to continue receiving HandHoverUpdate messages,
						// and prevent the hand from hovering over anything else
						hand.HoverLock( GetComponent<Interactable>() );

						// Attach this object to the hand
						hand.AttachObject( gameObject, attachmentFlags );
						// turn off hint while in hand
						directorHint.gameObject.SetActive (false);
					}
					else
					{
						// Detach this object from the hand
						hand.DetachObject( gameObject );
						// Call this to undo HoverLock
						hand.HoverUnlock( GetComponent<Interactable>() );
						// Restore position/rotation
						transform.position = oldPosition;
						transform.rotation = oldRotation;
					}
				}
			}
		}


		//-------------------------------------------------
		// Called when this GameObject becomes attached to the hand
		//-------------------------------------------------
		private void OnAttachedToHand( Hand hand )
		{
			textMesh.text = "";
		}


		//-------------------------------------------------
		// Called when this GameObject is detached from the hand
		//-------------------------------------------------
		private void OnDetachedFromHand( Hand hand )
		{
			textMesh.text = "Orb found! \nGrab it with the grip button!";
		}


		//-------------------------------------------------
		// Called every Update() while this GameObject is attached to the hand
		//-------------------------------------------------
		private void HandAttachedUpdate( Hand hand )
		{
			textMesh.text = "Press touchpad to collect.";
			// NonVR testing of collection:
			if (Input.GetKeyDown("x"))
			{
				CLE.OrbHunt.DebugWrapper.Debug ("collected by experimenter");
				hand.HoverUnlock( GetComponent<Interactable>() );
				hand.DetachObject( gameObject );
				if (CLE.OrbHunt.EnvironmentGameManager.gm != null) //if there is a gm (object of the type GameManager)
				{
					// tell the environment game manager to Collect the Orb on all PCs
					CLE.OrbHunt.EnvironmentGameManager.gm.gmPhotonView.RPC ("Collect", PhotonTargets.AllBufferedViaServer, orbValue);
				}
			}
			if (hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
			{
				CLE.OrbHunt.DebugWrapper.Debug ("Touchpad pressed while hand attached (update)");
				hand.DetachObject( gameObject );
				// explode if specified
				if (explosionPrefab != null)
				{
					PhotonNetwork.Instantiate (explosionPrefab.name, CLE.OrbHunt.RoundManager.orbInstance.transform.position, Quaternion.identity,0);
					CLE.OrbHunt.DebugWrapper.Debug ("orb is collected and disappears");
				}
				transform.position = new Vector3 (0,0,0);
				hand.HoverUnlock( GetComponent<Interactable>() );

				if (CLE.OrbHunt.EnvironmentGameManager.gm != null) //if there is a gm (object of the type GameManager)
				{
					// tell the environment game manager to Collect the Orb on all PCs
					CLE.OrbHunt.EnvironmentGameManager.gm.gmPhotonView.RPC ("Collect", PhotonTargets.AllBufferedViaServer, orbValue);
				}
			}
		}


		//-------------------------------------------------
		// Called when this attached GameObject becomes the primary attached object
		//-------------------------------------------------
		private void OnHandFocusAcquired( Hand hand )
		{
		}


		//-------------------------------------------------
		// Called when another attached GameObject becomes the primary attached object
		//-------------------------------------------------
		private void OnHandFocusLost( Hand hand )
		{
		}
			
	}
}
