﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Debug = FFP.Debug;

/*! ----------------------------------------------------------------------------
 * Main hud control, contains functions for updating HUD information on the player's screen
 * these functions are designed to be called from whatever script needs to update them.
 * ----------------------------------------------------------------------------
 */
[EventVisible("UI")]
public class GameHUD : MonoBehaviour {
	#region singletonEnforcement
	private static GameHUD instance;
	public static GameHUD Instance {
		get {
			return instance;
		}
		private set { }
	}
	#endregion

#pragma warning disable 0219
#pragma warning disable 0414
    GameObject UIhud;
	GameObject mainHUDCanvas;				//!<The canvas HUD is rendered on
	GameObject worldMapCanvas;				//!<All the game map elements
	GameObject player;						//!<reference to player
	public GameObject pauseMenu;
	public PauseMenu accessManager;
    public MainMenuManager menuManager;
	public RenderTexture MiniMapRenderTexture;
	public Material MiniMapMaterial;
	public float minimapXOffset;
	public float minimapYOffset;
	
	GameObject mapCam;								//!<Camera used for minimap
	static GameObject objectiveText;						//!<Objective Text UI element

	GameObject[] mapLabels;							//!<Array of text taht appears on minimap

	public bool skillsOpen = false;
	bool canSpin = false;
	GameObject closeMapButton;
	GameObject phoneButtons;
	GameObject mapElements;
	GameObject compassCameraPoint;					//!<Point at camera location used to calculate objective positions
	GameObject compass;
	GameObject slider;
	GameObject leftArrow;
	GameObject rightArrow;
	GameObject journal;
	GameObject questManagerUI;
	GameObject minimapRedArrow; 					//!<Arrow for the player representation on the minimap
	GameObject mainCamera;
	GameObject minimapCamera;
	GameObject minimapCompass;
	GameObject testObjective;

	float offset = 10f;

	void Awake() {

		#region singletonEnforcement
		if(instance == null) {
			instance = this;
		} else {
			Destroy(this.gameObject);
			Debug.Error("core", "Second GameHUD detected. Deleting gameOject.");
			return;
		}
		#endregion

        UIhud = GameObject.Find("_UI");
		mainHUDCanvas = GameObject.Find("mainHUD");
		worldMapCanvas = GameObject.Find("worldMapCanvas");
		//gameMap = GameObject.Find("mapBG");
		player = GameObject.Find("_Player");

		if (!pauseMenu){
            pauseMenu = GameObject.Find("pauseMenu");
        }
		pauseMenu.SetActive (false);
		//!Turn on UI stuff
		worldMapCanvas.SetActive(true);

		//!Fill mapLabels array
		mapLabels = GameObject.FindGameObjectsWithTag("worldMapLabel");
		closeMapButton = GameObject.Find("CloseMapButton");

		if (closeMapButton) {
			closeMapButton.SetActive (false);
		}
		//!Set compassCameraPoint reference

		compassCameraPoint = GameObject.Find("compassCameraPoint");
		compass = GameObject.Find("compassSlider");
		slider = compass.transform.FindChild ("Handle Slide Area").gameObject;
		slider.SetActive (false);
		leftArrow = compass.transform.FindChild ("leftArrow").gameObject;
		leftArrow.SetActive (false);
		rightArrow = compass.transform.FindChild ("rightArrow").gameObject;
		rightArrow.SetActive (false);

		//!Set objective text reference
		objectiveText = GameObject.Find("objectiveText");

		phoneButtons = GameObject.Find("PhoneButtons");

		mapElements = GameObject.Find("MapElements");
		mapElements.SetActive(false);
		journal = GameObject.Find ("Journal");
		if (!journal) {
			print ("Could not find the 'Journal' GameObject in the current Scene: " + Application.loadedLevelName);
		} else {
			journal.SetActive (false);
		}

		questManagerUI = GameObject.Find ("QuestManagerUI");
		if (!questManagerUI) {
			print ("Could not find the 'QuestManagerUI' GameObject in the current Scene: " + Application.loadedLevelName);
		}
		minimapRedArrow = GameObject.Find ("MinimapRedArrow");
		if(!minimapRedArrow){
			print ("Could not find the 'MinimapRedArrow' GameObject in the current Scene: " + Application.loadedLevelName);
		}
		mainCamera = GameObject.Find ("_Main Camera");
		if(!mainCamera){
			print("Could not find the '_Main Camera' GameObject in the current Scene: " + Application.loadedLevelName);
		}
		minimapCamera = GameObject.Find ("MinimapCamera");
		if(!minimapCamera){
			print("Could not find the 'MinimapCamera' GameObject in the current Scene: " + Application.loadedLevelName);
		}
		minimapCompass = GameObject.Find ("MinimapCompass");
		if (!minimapCompass) {
			print("Could not find the 'MinimapCompass' GameObject in the current Scene: " + Application.loadedLevelName);
		}
	}

	void Start() {
		//Place the ability buttons in the Phone Menu
		//SpawnHudAbilityIcons ();
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape) && journal.activeSelf) {
			CloseJournal();
		}
	}

	void FixedUpdate() {
		//!This is for testing, call update map from player movements
		UpdateMapObjects();

		//!Set the compass indicator
		setCompassValue(calculateObjectiveAngle(testObjective));
	}

	void OnGUI(){
		//!Calls the DrawTexture function to display the minimap on the screen
		Graphics.DrawTexture (new Rect (minimapXOffset, Screen.height - 256 - minimapYOffset, 256, 256), MiniMapRenderTexture, MiniMapMaterial);
	}

	//!Call this to update objective tet at top of the screen
	[EventVisible]
	public void UpdateObjectiveText(string newObjective) {
		objectiveText.GetComponent<Text>().text = newObjective;
	}

	//!Rotates and moves all of the relevant objects on the minimap
	public void UpdateMapObjects() {
		Quaternion newRotation;
		foreach(GameObject curLabel in mapLabels) {
			RotateMapObjectsAboutGameObject(curLabel, mainCamera);
		}
		RotateMapObjectsAboutGameObject(minimapRedArrow, player);
		moveMapObjectsFromOtherObject(minimapRedArrow, player, new Vector3(0, -15, 0));
		RotateMapObjectsAboutGameObject (minimapCamera, mainCamera);
		moveMapObjectsFromOtherObject(minimapCamera, player, new Vector3(0, -10, 0));
		moveMapObjectsFromOtherObject (minimapCompass, player, new Vector3(0, -16, 0));
	}

	/*!Rotates items on the minimap always face in the correct direction
	 * The mainObject is the object that is to be rotated
	 * The secondaryObject is the object that the mainObject is to be rotated in reference to
	 * For example if you wanted to rotate a text object on the screen as the main camera turns,
	 * the text object would be the mainObject and the main camera would be the secondaryObject
	 */
	void RotateMapObjectsAboutGameObject(GameObject mainObject, GameObject secondaryObject){
		Quaternion newRotation;
		newRotation = Quaternion.Euler(new Vector3(90, 0, -secondaryObject.transform.rotation.eulerAngles.y));
		if(mainObject.GetComponent<RectTransform>()){
			mainObject.GetComponent<RectTransform>().rotation = newRotation;
		}
		else{
			mainObject.GetComponent<Transform>().rotation = newRotation;
		}
	}
	
	/*!Moves items on the minimap so they follow another object that is in the world
	 * The mainObject is the object to be moved, the secondaryObject is the object that the mainObject is following,
	 * and the positionOffest is an offset to have control over the depth of the minimap related objects.
	 */
	void moveMapObjectsFromOtherObject(GameObject mainObject, GameObject secondaryObject, Vector3 positionOffest){
		mainObject.transform.position = secondaryObject.transform.position + positionOffest;
	}

	public void setCompassValue(float newValue) {

		if (testObjective == null)
        {
            return;
        }

		//!Calculates distances between "the player and the objective" and "the camera and the objective"
		float distanceBetweenCamAndObj = Vector3.Distance (compassCameraPoint.transform.position, testObjective.transform.position);
		float distanceBetweenPlayerAndObj = Vector3.Distance (player.transform.position, testObjective.transform.position);

		//!If the camera is closer to the objective, this means the objective is behind the player.
		//!In these first two cases, the compass will be forced to one side or the other as to not confuse the player
		if (distanceBetweenCamAndObj < distanceBetweenPlayerAndObj && newValue >= 90) {
			newValue = 105;
		} 
		else if (distanceBetweenCamAndObj < distanceBetweenPlayerAndObj && newValue < 90) {
			newValue = 75;
		}

		else if(newValue > 105) {
			newValue = 105;
		}
		else if(newValue < 75) {
			newValue = 75;
		}

		if (newValue == 105) {
			slider.SetActive (false);
			rightArrow.SetActive (true);
			leftArrow.SetActive(false);
		} 
		else if (newValue == 75) {
			slider.SetActive (false);
			leftArrow.SetActive (true);
			rightArrow.SetActive(false);
		} 
		else {
			leftArrow.SetActive(false);
			rightArrow.SetActive(false);
			slider.SetActive(true);
		}
		compass.GetComponent<Slider>().value = newValue;
	}

	public float calculateObjectiveAngle(GameObject objective) {

        if (objective == null)
        {
            return 0;
        }
		Vector3 pointToObjective;
		Vector3 pointStraightFromCam;

		//!Set points to determine which side of the player the vector towards the objective is going
		GameObject camPointRight = GameObject.Find("camPointRight");
		GameObject camPointleft = GameObject.Find("camPointLeft");


		//!create vector3 from player to objective and normalize it
		pointToObjective = objective.gameObject.transform.position - compassCameraPoint.transform.position;
		pointToObjective.Normalize();

		//!Set this vector to point right away from camera
		pointStraightFromCam = -compassCameraPoint.transform.right;
		pointStraightFromCam.Normalize();

		//!return angle from right facing vector

		return Vector3.Angle(pointStraightFromCam, pointToObjective);


	}

	//This is for testing
	void OnTriggerEnter(Collider col) {
		if(col.gameObject.tag == "Finish") {
			UpdateObjectiveText("Objective Complete!");
		}
	}


	//Shows map on phone and roates and resizes phone to screen
	public void showMap() {
		phoneButtons.SetActive(false);
		mapElements.SetActive(true);
		closeMapButton.SetActive(true);
		GameObject.Find("PhoneMenu").GetComponent<Animator>().SetBool("mapActive", true);
	}

	//hides map and plays close phone animation
	public void hideMap() {
		mapElements.SetActive(false);
		phoneButtons.SetActive(true);
		closeMapButton.SetActive(false);
		GameObject.Find("PhoneMenu").GetComponent<Animator>().SetBool("mapActive", false);

	}

	public void ChangeInputToUI(bool change = true) {/*
		if(change)
		InputManager.instance.ChangeInputType("UIInputType");
		else
			InputManager.instance.ChangeInputType("GameInputType");*/
	}

    public void PauseNoMenu() {
        accessManager.setPause();
        
    }

    public void timeNormal() {
        accessManager.setTimeNormal();
    }

    public void timeManipulate(float speed) {
        //speed = 2.0f;
        if(speed > 0 && speed < 1.75) {
            accessManager.manipulateTime(speed);
        } 
        else {
            System.Console.WriteLine("Error value of Speed GameHUD :: timeManipulate(float speed)");
        }
        
     }

	public void showPauseMenu () {
		pauseMenu.SetActive (true);
		
	}

	public void hidePauseMenu () {
		pauseMenu.SetActive (false);
		accessManager.unPauseGameBtt();
	}

	public void loadScene(string s) {
		Application.LoadLevel(s);
	}

	//!When this function is called the game closes. Only works in a build of the game
	public void quitGame () {
		Application.Quit ();
	}
	
    public void openOptions() 
    {
        menuManager.GoToOptions();
    }

	/*!This function activates the journal and deactivates the pause menu
	 * The function also highlights the quests button (the first item in the journal)
	 */
	public void ShowJournal(){
		journal.SetActive (true);
		accessManager.isOnPauseMenu = false;
		journal.transform.FindChild ("MainScrollView").FindChild ("JournalItems").FindChild ("QuestsItem").GetComponent<Button>().Select();
		pauseMenu.SetActive (false);
	}

	/*!This function deactivates the journal and activates the pause menu
	 */
	public void CloseJournal(){
		journal.SetActive (false);
		pauseMenu.SetActive (true);
		accessManager.isOnPauseMenu = true;
	}

	/*!This function activates the QuestManagerUI and deactivates the Journal
	 */
	public void ShowQMUI(){
		questManagerUI.SetActive (true);
		questManagerUI.GetComponent<QuestManagerUIController> ().showQuests ();
		journal.SetActive (false);
	}
	/*!This function deactivates the QuestManagerUI and activates the Journal
	 */
	public void HideQMUI(){
		questManagerUI.SetActive (false);
		journal.SetActive (true);
		journal.transform.FindChild ("MainScrollView").FindChild ("JournalItems").FindChild ("QuestsItem").GetComponent<Button>().Select();
	}
}