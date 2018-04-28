using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Preview : abs_gui {

	[SerializeField] private GameObject IT_target;
	[SerializeField] private GameObject AR_camera;
	[SerializeField] private GameObject GO_camera;
	[SerializeField] private GameObject GO_model;
	[SerializeField] private GameObject GO_marker;

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


		// Preview
		if (GUI.Button (new Rect (10, 10, 80, 32), "Finished")) {
			base.MasterController().GetComponent<scr_SceneController>().mouseState = scr_SceneController.MouseState.EDITING;
			base.MasterController().GetComponent<scr_SceneController>().SetCurrentProgState (scr_SceneController.ProgState.EDIT);
			GO_marker.GetComponent<MeshRenderer> ().enabled = true;
			GO_model.GetComponent<MeshRenderer> ().enabled = true;
			IT_target.transform.localScale = new Vector3 (1f, 1f, 1f);
			GO_camera.SetActive (true);
			GO_camera.GetComponent<Camera> ().enabled = true;
			AR_camera.SetActive (false);
			AR_camera.GetComponent<Camera> ().enabled = false;
			this.GetComponent<scr_Edit> ().ShowGUI (true);
			this.GetComponent<scr_Preview> ().ShowGUI (false);
			return;
		}
	}
}
