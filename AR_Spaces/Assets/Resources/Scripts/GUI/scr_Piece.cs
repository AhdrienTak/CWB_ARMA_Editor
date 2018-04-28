using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Piece : abs_gui {

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


		//Create a Back button
		if (GUI.Button (new Rect (10, 10, 80, 32), "Back")) {
			base.MasterController().GetComponent<scr_SceneController>().ChangeState (scr_SceneController.ProgState.GALLERY);
			StartCoroutine (base.MasterController().GetComponent<scr_RefreshListings>().Activate());
			return;
		}
		// Create Top Buttons
		GUI.Label (new Rect (x0 - 100, 34, 256, 32), "Please select a piece.");
		GUI.Label (new Rect (x0 + 100, 34, 256, 32), base.MasterController().GetComponent<scr_SceneController>().uMess);
		base.MasterController().GetComponent<scr_SceneController>().fName = 
			GUI.TextField (new Rect (x0 - 30, 70, 256, 32), base.MasterController().GetComponent<scr_SceneController>().fName, 32);
		if (GUI.Button (new Rect (x0 - 240, 70, 200, 32), "Create new piece")) {
			StartCoroutine (base.MasterController().GetComponent<scr_CreateDirectory>().Activate());
		}
		// Procedurally create buttons for each file
		int current = 0;
		int offset = 1;
		foreach (string item in base.MasterController().GetComponent<scr_SceneController>().listings) {
			// Ensure the correct buttons are visible on screen
			if (current <  base.MasterController().GetComponent<scr_SceneController>().indexCur)
				continue;
			if (current >= base.MasterController().GetComponent<scr_SceneController>().indexMax)
				break;
			// Check if directory is live, and create the toggle button
			string live = item.Substring(item.Length - 1);
			string name = item.Substring(0, item.Length - 1);
			if (live.Equals ("x")) {
				GUI.backgroundColor = Color.red;
			}
			else {
				GUI.backgroundColor = Color.green;
			}
			if (GUI.Button (new Rect (x0 - 240, 70 + (offset * 36), 92, 32), "Live")) {
				StartCoroutine (base.MasterController().GetComponent<scr_LockDirectory>().Activate (name, live));
			}
			GUI.backgroundColor = Color.gray;
			// Create the buttons for each Gallery
			if (GUI.Button (new Rect (x0 - 132, 70 + (offset * 36), 220, 32), name)) {
				base.MasterController().GetComponent<scr_SceneController>().livePiece = live;
				StartCoroutine (base.MasterController().GetComponent<scr_OpenDirectory>().Activate (name));
			}
			if (GUI.Button (new Rect (x0 + 104, 70 + (offset * 36), 92, 32), "Delete")) {
				StartCoroutine (base.MasterController().GetComponent<scr_DeleteDirectory> ().Activate (name));
			}
			offset++;
			current++;
		}
		//TODO create a scroll bar
	}
}
