﻿using UnityEngine;
using System.Collections;

/*!
 *	Base Scene manager class. Each scene should have it's own SceneManager class
 */
public abstract class SceneManager : MonoBehaviour {
	public virtual void LoadNextExpectedScene() { }
	public virtual void LoadPreviousExpectedScene() { }

	public void ExitGame() {
		Application.Quit();
	}
}
