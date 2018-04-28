using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class abs_gui : MonoBehaviour {

	private GameObject MS_control;
	private bool isVisible;


	void Awake() {
		isVisible = false;
		MS_control = GameObject.Find ("MASTER_Controller");
	}

	public void ShowGUI(bool show) {
		isVisible = show;
	}

	public bool IsVisible() {
		return isVisible;
	}

	public GameObject MasterController() {
		return MS_control;
	}
}
