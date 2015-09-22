﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = FFP.Debug;

public class Quinc : MonoBehaviour {
	//this class is a cluster fuck and needs to be organized
#pragma warning disable 0414
	private float pushRate = 0.5f;
	public float pushDistance = 5.0f;
	public float pushModifier = 50.0f;
	private float nextPush = 0.0f;
	private int pushRange = 20;

	private float pullRate = 0.5f;
	public float pullDistance = 5.0f;
	public float pullModifier = 50.0f;
	private float nextPull = 0.0f;
	private int pullRange = 20;

	private int cutRange = 20;

	private int soundThrowRange = 20;

	private int stunRange = 20;

	private int heatRange = 20;

	private int coldRange = 20;

	private int blastRange = 20;	//maximum distance from player to trigger blast

	private int blastRadius = 10;	//size of blast

	public float smoothing = 1f;

	//! These variables are temporary for testing, can be removed when implementing Targeting into code
	public GameObject pushPullTarget;
	public GameObject cutTarget;
	public GameObject soundThrowTarget;
	public GameObject stunTarget;
	public GameObject heatColdTarget;
	public GameObject blastTarget;

	List <GameObject> acquiredTargets = new List<GameObject>();

	bool inTargetLock = false;

	public static int activeAbility = 0;

	public int testAbility = 0;


	void FixedUpdate()
	{

		activeAbility = testAbility;

/*COMMENTED OUT FOR TESTING, REMOVE COMMENTING FOR BUILD
		//check current ability with active ability
		if(GameHUD.Instance.curAbility != activeAbility)
		{
			activeAbility = GameHUD.Instance.curAbility;
		}
*/

		//Get the list of targeted objects and the index if in camera target mode

		//check camera state for targets
		if(PoPCamera.State == PoPCamera.CameraState.TargetLock)
		{
			//get list of targeted objects
			acquiredTargets = PoPCamera.instance.targetedObjects;

		}

		//if there are targeted objects in the list, allow actions on them
		if(acquiredTargets.Count != 0) //&& inTargetLock)
		{
			//if (Input.GetKeyDown(KeyCode.Alpha1) && Time.time > nextPush)
			if(InputManager.input.isActionPressed() && activeAbility == 0 && Time.time > nextPush)
			{
				nextPush = Time.time + pushRate;
				//pushPullTarget = GameObject.Find("Crate"); //!> Reference Targeting Script to get current Target

				//get current target
				pushPullTarget = PoPCamera.instance.CurrentTarget();

				//push object
				//push success/error string
				string pushStatus = "trying to push";
				//pass status string by reference and target game object to push
				if(Push(ref pushStatus, pushPullTarget))
				{
					//output success message
					print("Push status: " + pushStatus);
				}
				else
				{
					//output error message
					print("Push error: " + pushStatus);
				}//END if(Push(ref pushStatus, pushPullTarget))

			}
				//else if (Input.GetKeyDown(KeyCode.Alpha2) && Time.time > nextPull)
			else if(InputManager.input.isActionPressed() && activeAbility == 1 && Time.time > nextPull)
			{
				nextPull = Time.time + pullRate;
				//pushPullTarget = GameObject.Find("Crate"); //!> Reference Targeting Script to get current Target

				//get current target
				pushPullTarget = PoPCamera.instance.CurrentTarget();
				//pull object
				//push success/error string
				string pullStatus = "trying to pull";
				//pass status string by reference and target game object to pull
				if(Pull(ref pullStatus, pushPullTarget))
				{
					//output success message
					print("Pull status: " + pullStatus);
				}
				else
				{
					//output error message
					print("Pull error: " + pullStatus);
				}//END if(Pull(ref pullStatus, pushPullTarget))

			}
				//else if (Input.GetKeyDown(KeyCode.Alpha3))
			else if(InputManager.input.isActionPressed() && activeAbility == 2 && Input.GetKeyDown(KeyCode.I))
			{
				//cutTarget = GameObject.Find("Rope"); //!> Reference Targeting Script to get current Target


				cutTarget = PoPCamera.instance.CurrentTarget();

				//cut object
				//cut success/error string
				string cutStatus = "trying to cut";

				//pass status by reference and target game object to cut
				if(Cut(ref cutStatus, cutTarget))
				{
					//output success message
					print("Cut status: " + cutStatus);
				}
				else
				{
					//output error message
					print("Cut error: " + cutStatus);
				}

			}
				//else if(Input.GetKeyDown(KeyCode.Alpha4))
			else if(InputManager.input.isActionPressed() && activeAbility == 3 && Input.GetKeyDown(KeyCode.O))
			{
				string soundStatus = "";
				//soundThrowTarget = GameObject.Find("Well"); //!> Reference Targeting Script to get current Target


				soundThrowTarget = PoPCamera.instance.CurrentTarget();

				if(SoundThrow(ref soundStatus, soundThrowTarget)) {
					print("Sound Status: " + soundStatus);
				}
				else
				{
					print("Sound Error: " + soundStatus);
				}//END if(SoundThrow(ref soundStatus, soundThrowTarget))
	
			}
				//else if(Input.GetKeyDown(KeyCode.Alpha5))
			else if(InputManager.input.isActionPressed() && activeAbility == 4 && Input.GetKeyDown(KeyCode.P))
			{
				//stunTarget = GameObject.Find("Enemy"); //!> Reference Targeting Script to get current Target

	

				stunTarget = PoPCamera.instance.CurrentTarget();

				//stun object
				//stun success/error string
				string stunStatus = "trying to stun";

				//pass status by reference and target game object to cut
				if(Stun(ref stunStatus, stunTarget))
				{
					//output success message
					print("Stun status: " + stunStatus);
				}
				else
				{
					//output error message
					print("Stun error: " + stunStatus);
				}//END if(Cut(ref stunStatus, stunTarget))

			}
			else if(InputManager.input.isActionPressed() && activeAbility == 5)
			{

				heatColdTarget = PoPCamera.instance.CurrentTarget();

				//heat object
				string heatStatus = "trying to heat";

				//pass status by reference and target object to heat
				if(Heat(ref heatStatus, heatColdTarget))
				{
					//output success message
					print("Heat status: " + heatStatus);
				}
				else
				{
					//output error message
					print("Heat error: " + heatStatus);
				}
			}
			else if(InputManager.input.isActionPressed() && activeAbility == 6)
			{
				heatColdTarget = PoPCamera.instance.CurrentTarget();

				//heat object
				string coldStatus = "trying to heat";

				//pass status by reference and target object to heat
				if(Cold(ref coldStatus, heatColdTarget))
				{
					//output success message
					print("Cold status: " + coldStatus);
				}
				else
				{
					//output error message
					print("Cold error: " + coldStatus);
				}
			}
			else if(InputManager.input.isActionPressed() && activeAbility == 7)
			{

				blastTarget = PoPCamera.instance.CurrentTarget();

				//heat object
				string blastStatus = "trying to blast";

				//pass status by reference and target object to heat
				if(Blast(ref blastStatus, blastTarget))
				{
					//output success message
					print("Blast status: " + blastStatus);
				}
				else
				{
					//output error message
					print("Blast error: " + blastStatus);
				}
			}
		}//END if(acquiredTargets.Count != 0 && inTargetLock)

	}//END void FixedUpdate()

	//! Function to be called when pushing a box or other heavy object, pushing at intervals (think Ocarina of time)
	bool Push(ref string status, GameObject pushTarget) {
		/*
		Check if Object is Push Compatible
		Else return "Object not Compatible"
		Check if Object is with in Push Range
		Else return "Object not in Range"
		Push the object either with force, translation, or grid-based movement
		Play animation for player using Push
		*/
		print("Distance between Player and Target: " + Vector3.Distance(pushTarget.transform.position, transform.position));

		if(pushTarget == null)
		{
			status = "No Target Selected";
			return false;
		}
		if(!pushPullTarget.GetComponent<Item>().pushCompatible)
		{
			status = "Not Compatible";
			return false;
		}
		if(Vector3.Distance(pushTarget.transform.position, transform.position) > pushRange)
		{
			status = "Not In Range";
			return false;
		}

		/* 
			1. Get direction to push
			2. Get position by taking direction multiplied by distance
			3. Use Raycast from Heaven to find Y value for targetPosition that is valid or is above terrain
			4. Call Coroutine to Lerp to target position
		*/

		Vector3 pushDirection = pushTarget.transform.position - transform.position;
		pushDirection.Normalize();
		Vector3 targetPosition = (pushDirection * pushDistance) + pushTarget.transform.position;
		StartCoroutine(MoveSlowly(pushTarget.gameObject, targetPosition, pushDirection));
		status = "Push Successful";
		return true;
	}

	//! Function to be called when pulling an object, pulling at intervals (think Ocarina of time)
	bool Pull(ref string status, GameObject pullTarget)
	{
		/*
		Check if Object is PullCompatible
		Else return "Object not Compatible"
		Check if Object is with in Pull Range
		Else return "Object not in Range"
		Pull the object either with force, translation, or grid-based movement
		Play animation for player using Pull
		*/

		if(pullTarget == null)
		{
			status = "No Target Selected";
			return false;
		}
		if(!pullTarget.GetComponent<Item>().pullCompatible)
		{
			status = "Not Compatible";
			return false;
		}
		if(Vector3.Distance(pullTarget.transform.position, transform.position) > pullRange)
		{
			status = "Not In Range";
			return false;
		}

		Vector3 pullDirection = transform.position - pullTarget.transform.position;
		pullDirection.Normalize();
		Vector3 targetPosition = (pullDirection * pullDistance) + pullTarget.transform.position;
		StartCoroutine(MoveSlowly(pullTarget.gameObject, targetPosition, pullDirection));
		status = "Pull Successful";
		return true;

	}

	//! Function to be called when cutting rope
	bool Cut(ref string status, GameObject cutTarget)
	{
		/*
		Check if Object is Cut Compatible
		Else return "Object not Compatible"
		Check if Object is within Cut Range
		Else return "Object not in Range"
		Call Object.cut() function
		Play animation for player using Cut
		Untarget GameObject?
		*/
		if(cutTarget == null)
		{
			status = "No Target Selected";
			return false;
		}
		if(!cutTarget.GetComponent<Item>().cutCompatible)
		{
			status = "Not Compatible";
			return false;
		}
		if(Vector3.Distance(cutTarget.transform.position, transform.position) > cutRange)
		{
			status = "Not In Range";
			return false;
		}


		cutTarget.GetComponent<Rope>().Cut();

		// Untarget Cuttable Object?
		status = "Cut Successful";
		return true;

	}

	//! Function that activates SoundThrow app on Quinc phone that distracts enemies
	bool SoundThrow(ref string status, GameObject soundThrowTarget)
	{
		/*
		Check if Object is SoundThrow compatible
		Else return "Object not Compatible"
		Check if Object is within SoundThrow Range
		Else return "Object not in Range"
		Maybe pick type of sound to be thrown?
		Play sound and animation
		Untarget GameObject?
		*/
		if(soundThrowTarget == null)
		{
			status = "No Target Selected";
			return false;
		}
		if(!soundThrowTarget.GetComponent<Item>().soundThrowCompatible)
		{
			status = "Not Compatible";
			return false;
		}
		if(Vector3.Distance(soundThrowTarget.transform.position, transform.position) > soundThrowRange)
		{
			status = "Not In Range";
			return false;
		}

		// Possibly allow player to select sound, or each object will have it's own sound attached to it
		// Call SoundThrow function in Well script, which will play sound and animation
		soundThrowTarget.GetComponent<Well>().SoundThrow();

		// Untarget GameObject?
		status = "SoundThrow Successful";
		return true;
	}

	//! Function called when activating Stun App on Quinc phone. Only works against enemies.
	bool Stun(ref string status, GameObject stunTarget)
	{
		/*
		Check if Object is Stunnable object
		Else return "Object not Compatible"
		Check if Object is within Stun Range
		Else return "Object not in Range"
		Call Object.stun() funciton
		Play animation for player using stun
		Untarget GameObject?
		*/

		if(stunTarget == null)
		{
			status = "No Target Selected";
			return false;
		}
		if(!stunTarget.GetComponent<Item>().stunCompatible)
		{
			status = "Not Compatible";
			return false;
		}
		if(Vector3.Distance(stunTarget.transform.position, transform.position) > stunRange)
		{
			status = "Not In Range";
			return false;
		}

		stunTarget.GetComponent<Enemy>().Stun();

		//Untarget Enemy?
		status = "Stun Successful";
		return true;
	}

	bool Heat(ref string status, GameObject heatTarget)
	{

		if(heatTarget == null)
		{
			status = "No Target Selected";
			return false;
		}
		if(!heatTarget.GetComponent<Item>().heatCompatible)
		{
			status = "Not Compatible";
			return false;
		}
		if(Vector3.Distance(heatTarget.transform.position, transform.position) > heatRange)
		{
			status = "Not In Range";
			return false;
		}

//DO SOMETHING HERE TO HEAT WATER

		status = "Heat Successful";
		return true;

	}//END bool Heat(ref string status, GameObject heatTarget)

	bool Cold(ref string status, GameObject coldTarget)
	{

		if(coldTarget == null)
		{
			status = "No Target Selected";
			return false;
		}
		if(!coldTarget.GetComponent<Item>().coldCompatible)
		{
			status = "Not Compatible";
			return false;
		}
		if(Vector3.Distance(coldTarget.transform.position, transform.position) > coldRange)
		{
			status = "Not In Range";
			return false;
		}

//DO SOMETHING HERE TO FREEZE WATER

		status = "Cold Successful";
		return true;

	}

	bool Blast(ref string status, GameObject blastTarget)
	{

		if(blastTarget == null)
		{
			status = "No Target Selected";
			return false;
		}
		if(!blastTarget.GetComponent<Item>().blastCompatible)
		{
			status = "Not Compatible";
			return false;
		}
		if(Vector3.Distance(blastTarget.transform.position, transform.position) > blastRange)
		{
			status = "Not In Range";
			return false;
		}

//DO SOMETHING HERE TO DISAPPEAR

		//if dark
		//create light blast

		//if light
		//create smoke blast

		status = "Blast Successful";
		return true;

	}

	IEnumerator MoveSlowly(GameObject targetObject, Vector3 targetPosition, Vector3 direction)
	{
		print("Target Position In CoRoutine: " + targetPosition);

		while(Vector3.Distance(targetObject.transform.position, targetPosition) > 2.0f)
		{
			targetObject.transform.position = Vector3.Lerp(targetObject.transform.position, targetPosition, smoothing * Time.deltaTime);
			yield return null;
		}
		yield return null;

		print("Target Reached");
	}

}
