﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = FFP.Debug;

[RequireComponent(typeof(NavMeshAgent))]	//Automaticly Make a navMeshAgent on this game object when this script is applied
[RequireComponent(typeof(Rigidbody))]
/*! 
 *	This code is the main ai controller. The class is segmented into the following regions: attack, movement, state, sight, and health
 */
[EventVisible]
public class AIMainTrimmed : MonoBehaviour {

	//Variables Editable

	public int 					hp 					= 100;						//!<Health of the NPC
	public float 				sightDistance 		= 45;						//!<The distance the NPC is capable of seeing
	public float 				sightAngle			= 45;						//!<The max angle of the cone of vision
	public float 				speed 				= 5;						//!<Casual Speed of the NPC
	public float		 		runSpeed 			= 7;						//!<Scared, Charging, Aggressive Speed of the NPC*
	public string[]		 		seekTag 			= {"Player"};				//!<The enemy Tag of this NPC
	public float 				attackDistance		= 3;						//!<The distance the NPC stands away from the target and attacks*
	public float 				aggressionLimit		= 100;						//!<The aggression level of the attacker
    public float                suspicionLimit      = 150;						//!<The suspicon level of the attacker
	public GameObject 			startPoint 			= null;						//!<Sets the first navPoint, defaults to stationary
	public string				panicPoints			= "PanicPoints";			//!<The object name of the vector keeper of related panic points for the AI
	public bool 				aggressive 			= false;					//!<If the NPC will attack the player
    public GameObject           PlayerLastPos       = null;                     //visual representation of where the AI last remembers seeing the player before they went of LoS
    public bool                 searching           = false;                    //if the AI is currently looking for the player at its shadow
    public bool                 checking            = false; 


	//Variables Controllers
	
	public bool 		        seesTarget 			= false;					//!<If the Player has been spotted
	[ReadOnly]public GameObject target				= null;						//!<The transform of the player
    [ReadOnly]public GameObject temptarget          = null;                     //!Temp target for the shadowPlayer
    public bool 		        attacking 			= false;					//!<If the AI is attacking
	[ReadOnly]public bool 		panic 				= false;					//!<If the AI is panicking
	[ReadOnly]public Vector3 	panicTarget			= new Vector3 (0, 0, 0);	//!<Target of AI panic
	[ReadOnly]public float 		aggressionLevel 	= 0;						//!<The current awareness of the NPC to the Player
    [ReadOnly]public float      suspicionLevel      = 0;						//!<The current suspicion of the NPC to the Player	
    public bool                 suspicious          = false;					//!<If the AI is suspicous
    public bool                 alert               = false;					//!<If the AI is alert
    public bool                 unconsious          = false;                    //!<If the AI is unconsious
    public bool                 dazed               = false;                    //!<If the AI is dazed


	//Movement variables

	private NavMeshAgent 		mesh 				= null;						//!<Contains the component to use the navmesh
	[ReadOnly]public string		navCheck			= null;						//!<The name of the current checkpoint
	[ReadOnly]public Vector3 	navPoint			= new Vector3 (0, 0, 0);	//!<Contains the point to move in the navmesh

	//Sight variables

	private RaycastHit 			hit;											//!<Takes information from RayCast
	private GameObject[] 		viableTargets;									//!<All the available targets for the AI

	void Start() 
	{
        gameObject.GetComponent<Renderer>().material.color = Color.magenta;
        if (aggressionLevel >= aggressionLimit)
            GetComponent<Rigidbody> ().isKinematic = true;
        mesh = GetComponent<NavMeshAgent>();
        #region StartMoving
        if (startPoint != null)
			ChangeNavPoint(startPoint.name,startPoint.transform.position);	
		else
			ChangeNavPoint(this.name,this.transform.position);
		SetSpeed(speed);
        #endregion
    }


    void Update() 
	{ 
		if (hp > 0)
        {
            if (dazed == false) 
            {
                mesh.SetDestination(navPoint);
                #region attack
                if (target != null)
                {
                    if (attackDistance >= Vector3.Distance(transform.position, target.transform.position))
                    {
                        //if the target name is the player initiate reset of level
                        //if target name is shadowPlayer then destroy it. 
                        if (target.name == "shadowPlayer(Clone)")
                        {
                            Destroy(target);
                        }
                        target = null;
                    }
                }
                #endregion

                #region sight
                if (aggressive == true)
                {
                    StartCoroutine("CheckForTargets");
                }
                #endregion
            }
            else
            {
                
            }
		}
        else
        {
            Pause();
        }
	}


    
	void FixedUpdate ()
    {
        #region state
        if (panic == true)
        {      
            if (aggressive == true)
            {
                panic = false;
            }
            else
            {
                ChangeNavPoint("panic", panicTarget);
                SetSpeed(runSpeed);
            }
        }
        if (aggressive == true && seesTarget == true)
        {
            if (alert == true)
            {
                aggressionLevel = aggressionLimit;
            }
            else
            {
                if (suspicionLevel >= suspicionLimit)
                {
                    suspicious = true;
                    if (aggressionLevel >= aggressionLimit)
                    {
                        attacking = true;
                        ChangeNavPoint(target.name, target.transform.position);
                        SetSpeed(runSpeed);
                    }
                    else
                    {
                        aggressionLevel += 1;
                    }
                }
                else
                {
                    suspicionLevel += 1;
                }
            }
        }
        else
        {
            if (attacking == true && searching == false)
            {
                target = (GameObject)Instantiate(PlayerLastPos, target.transform.position, Quaternion.identity);
                searching = true;
            }
            if (suspicionLevel > 0)
            {
                StartCoroutine("Decrementsuspicion");
            }
            if (aggressionLevel > 0)
            {
                StartCoroutine("Decrementaggression");
            }
        }
        #endregion
        
        if (suspicious == true && attacking == true)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            if (suspicious == true)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    public IEnumerator EndAttack() 
	{
        yield return new WaitForSeconds (1);
		attacking = false;
    }

    public IEnumerator Decrementaggression()
    {
        yield return new WaitForSeconds(3f);
        if (aggressionLevel > 0)
        {
            aggressionLevel -= 1;
        }
        if (aggressionLevel == 0)
            {
                searching = false;
                attacking = false;
                ChangeNavPoint(startPoint.name, startPoint.transform.position);
            }
    }

    public IEnumerator Decrementsuspicion()
    {
        yield return new WaitForSeconds(3f);
        if (suspicionLevel > 0)
        {
            suspicionLevel -= 1;

        }
        if (suspicionLevel == 0)
        {
            suspicious = false;
        }
    }

    #region dazed

    //turn off line of sight (assuming visible LoS is a feature)
    //after x number of seconds
    //stop being dazed (animation)
    //start search sequence
    //turn left and right to look around
    //after set amount of time set its target to its last target
    //set dazed to false


    #endregion

    #region distracted

    //if noiseHeard = true (triggered either here through send message or in the throw script)
    //if enemymy does not see player
    //store last target
    //set location of noise from noise throw
    // if current location = target location
    //start search sequence

    /*if (noiseHeard == true)
    {
        if (seesTarget = false)
        {
            navPoint = (location fed from noise throw);
            mesh.SetDestination(navPoint);
            if (transform.position == navPoint)
            {
                Investigate();
            }
        }
        else
        {

        }
    }
    else
    {
    }
    */

    #endregion

    #region Move functions
    public void ChangeNavPoint(string N,Vector3 T)
    {
        navCheck = N;
        navPoint = T;
    }
    public void SetSpeed(float Spd)
    {
        mesh.speed = Spd;
    }
    public void Pause()
    {
        mesh.Stop();
    }
    public void Resume()
    {
        mesh.Resume();
    }
    #endregion

    #region Sight Functions
    public IEnumerator CheckForTargets()
    {
        yield return new WaitForSeconds(.1f);
        GetTargets();
    }
    public void GetTargets()
    {
        List<GameObject> viableTargets = new List<GameObject> ();
        for(int enemyType = 0; enemyType < seekTag.Length; enemyType++)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(seekTag[enemyType]);
            for(int searchEnemyType = 0;searchEnemyType < targets.Length; searchEnemyType++)
            viableTargets.Add(targets[searchEnemyType]);
        }

        if(viableTargets.Count != 0)
        {
            //check for and if need be.
            for (int searchTargets = 0; searchTargets < viableTargets.Count; searchTargets++)
            {
                if (Vector3.Angle(viableTargets[searchTargets].transform.position - transform.position, transform.forward) < sightAngle)
                {
                    if (Vector3.Distance(transform.position, viableTargets[searchTargets].transform.position) < sightDistance)
                    {
                        if (Physics.Raycast(transform.position, viableTargets[searchTargets].transform.position - transform.position, out hit))
                        {
                            if (hit.collider.tag == viableTargets[searchTargets].tag)
                            {
                                if (target != null && target.name == "shadowPlayer(Clone)")
                                {
                                    Destroy(target);
                                    CheckForTargets();


 
                                }
                                target = hit.collider.gameObject;
                                searching = false;
                                seesTarget = true;
                            }
                            else
                            {
                                seesTarget = false;
                            }
                        }
                        else
                        {
                            seesTarget = false;
                        }
                    }
                    else
                    {
                        seesTarget = false;
                    }
                }
                else
                {
                    seesTarget = false;
                }
            }
        }
        else
        {
        }
    }
    #endregion

    #region health
    public int hurt (int damage) 
    {
        int HP = hp;
        HP -= damage;
        return HP;
    }
    public int heal (int health) 
    {
        int HP = hp;
        HP += health;
        return HP;
    }
    #endregion

    #region Recover Functions
    public IEnumerator statusRecover()
    {
        yield return new WaitForSeconds(10.0f);
        dazeRecover();
    }

    public void dazeRecover()
    {
        //dazed == false
        //whatever actions need to be performed after recovery
        //ie. high suspicion level, aggressive, look around? contine patrol?
    }
    #endregion

    public void Investigate()
    {
        
        //play animation
        //after animation ChangeNavPoint(startPoint.name, startPoint.transform.position);
        //set noiseHeard to false
        //Searching = false;
    }
}