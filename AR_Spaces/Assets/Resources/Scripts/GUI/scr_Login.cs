using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Login : abs_gui {

	private float  x0;
	private float  y0;
	private string uName;
	private string uPass;


	public void PassVars(float x, float y, string n, string p) {
		x0 = x;
		y0 = y;
		uName = n;
		uPass = p;
	}

	void OnGUI() {
		if (!base.IsVisible ())
			return;
		if (base.MasterController().GetComponent<scr_SceneController>().CheckLocks())
			GUI.enabled = false;
		else
			GUI.enabled = true;


		// Create Labels
		GUI.Label (new Rect (x0 - 256, y0 - 70, 256, 32), "Please Log In.");
		GUI.Label (new Rect (x0 - 200, y0 - 32, 128, 32), "User Name: ");
		GUI.Label (new Rect (x0 - 200, y0, 128, 32), " Password: ");
		GUI.Label (new Rect (x0 - 256, y0 + 64, 512, 32), base.MasterController().GetComponent<scr_SceneController>().uMess);
		// Create Text Fields
		uName = GUI.TextField (new Rect (x0 - 128, y0 - 32, 256, 32), uName, 32);
		uPass = GUI.PasswordField (new Rect (x0 - 128, y0, 256, 32), uPass, '*', 32);
		// Create New Account
		if (GUI.Button (new Rect (x0 - 100, y0 + 50, 256, 32), "Create Account")) {
			StartCoroutine (base.MasterController().GetComponent<scr_CreateCredentials> ().Activate (uName, uPass));
		}
		// Create Submit Button
		if (GUI.Button (new Rect (x0 + 128, y0 - 32, 64, 64), "Log In")) {
			StartCoroutine (base.MasterController().GetComponent<scr_VerifyCredentials> ().Activate (uName, uPass));
		}
	}

}
