using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Deletion : abs_gui {

	private float  x0;
	private float  y0;
	private string uName;


	public void PassVars(float x, float y, string n) {
		x0 = x;
		y0 = y;
		uName = n;
	}

	void OnGUI() {
		if (!base.IsVisible ())
			return;
		if (base.MasterController().GetComponent<scr_SceneController>().CheckLocks())
			GUI.enabled = false;
		else
			GUI.enabled = true;


		// Get folder type
		string type;
		if (base.MasterController().GetComponent<scr_SceneController>().GetPreviousProgState() == scr_SceneController.ProgState.GALLERY)
			type = "Gallery";
		else if (base.MasterController().GetComponent<scr_SceneController>().GetPreviousProgState() == scr_SceneController.ProgState.PIECE)
			type = "Piece";
		// Create Labels
		GUI.Label (new Rect (50, y0 - 70, 512, 32), base.MasterController().GetComponent<scr_SceneController>().uMess);
		// Create buttons
		if (GUI.Button (new Rect (50, y0 - 40, 64, 32), "No")) {
			StartCoroutine (base.MasterController().GetComponent<scr_DeleteDirectory> ().Activate (false));
		}
		if (GUI.Button (new Rect (160, y0 - 40, 64, 32), "Yes")) {
			StartCoroutine (base.MasterController().GetComponent<scr_DeleteDirectory> ().Activate (true));
		}
	}
}
