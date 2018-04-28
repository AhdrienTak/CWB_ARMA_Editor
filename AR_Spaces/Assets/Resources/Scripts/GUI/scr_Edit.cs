using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crosstales.FB;
using System;
using System.IO;

public class scr_Edit : abs_gui {

	[SerializeField] private GameObject AR_camera;
	[SerializeField] private GameObject GO_camera;
	[SerializeField] private GameObject GO_light;
	[SerializeField] private GameObject GO_model;
	[SerializeField] private GameObject GO_marker;
	[SerializeField] private TextAsset defaultModel;

	private float  x0;
	private float  y0;
	private string uName;

	private string   tSpecularity;
	private string   tIntensity;
	private string[] tx_mod;
	private string[] tx_cam;
	private string[] tx_lit;
	private string[] lite_col;


	void Start() {
		tSpecularity = "0.1";
		tIntensity = "0.5";
		tx_mod = new string[] {
			"0.0", "0.0", "0.0",
			"0.0", "0.0", "0.0",
			"1.0", "1.0", "1.0",
		};
		tx_cam = new string[] {
			"0.0", "0.0", "0.0",
			"0.0", "0.0", "0.0",
			"1.0", "1.0", "1.0",
		};
		tx_lit = new string[] {
			"0.0", "0.0", "0.0",
			"0.0", "0.0", "0.0",
			"1.0", "1.0", "1.0",
		};
		lite_col = new string[] {
			"255", "255", "255" 
		};
	}

	public void PassVars(float x, float y, string n) {
		x0    = x;
		y0    = y;
		uName = n;

		for (int i = 0; i < 9; i++) {
			tx_mod[i]   = base.MasterController ().GetComponent<scr_SceneController> ().txform_mod[i];
			tx_cam[i]   = base.MasterController ().GetComponent<scr_SceneController> ().txform_cam[i];
			tx_lit[i]   = base.MasterController ().GetComponent<scr_SceneController> ().txform_lit[i];
			if (i > 2) continue;
			lite_col[i] = base.MasterController ().GetComponent<scr_SceneController> ().color_lite[i];
		}
		tSpecularity = base.MasterController ().GetComponent<scr_SceneController> ().specularity + "";
		tIntensity   = base.MasterController ().GetComponent<scr_SceneController> ().intensity;

		GO_light.GetComponent<Light>().color = new Color (
			(1.0f * int.Parse(base.MasterController ().GetComponent<scr_SceneController> ().color_lite[0])) / 255.0f,
			(1.0f * int.Parse(base.MasterController ().GetComponent<scr_SceneController> ().color_lite[1])) / 255.0f,
			(1.0f * int.Parse(base.MasterController ().GetComponent<scr_SceneController> ().color_lite[2])) / 255.0f
		);
		GO_light.GetComponent<Light>().intensity = float.Parse(base.MasterController ().GetComponent<scr_SceneController> ().intensity);

	}

	void OnGUI() {
		if (!base.IsVisible ())
			return;
		if (base.MasterController().GetComponent<scr_SceneController>().CheckLocks())
			GUI.enabled = false;
		else
			GUI.enabled = true;


		// Hide/Show GUI
		if (GUI.Button (new Rect ((Screen.width / 2) - 65, 10, 120, 32), "Hide/Show GUI")) {
			base.MasterController().GetComponent<scr_SceneController>().showEditGUI = !base.MasterController().GetComponent<scr_SceneController>().showEditGUI;
		}
		if (base.MasterController().GetComponent<scr_SceneController>().showEditGUI) {
			// Create a Back button
			if (GUI.Button (new Rect (10, 10, 80, 32), "Back")) {
				base.MasterController().GetComponent<scr_SceneController>().ChangeState (scr_SceneController.ProgState.PIECE);
				GO_model.GetComponent<MeshRenderer> ().enabled = false;
				GO_marker.GetComponent<MeshRenderer> ().enabled = false;
				StartCoroutine (base.MasterController().GetComponent<scr_RefreshListings>().Activate());
				return;
			}

			// Select a 3D model
			if (GUI.Button (new Rect (32, 65, 128, 24), "Select model")) {
				string selectedFile = FileBrowser.OpenSingleFile ("Select .obj", base.MasterController().GetComponent<scr_SceneController>().fp_object, "obj");
				if (!string.IsNullOrEmpty (selectedFile)) {
					base.MasterController().GetComponent<scr_SceneController>().fp_object = selectedFile;
					base.MasterController().GetComponent<scr_SceneController>().UpdateFilePaths ();
					GO_model.GetComponent<MeshFilter> ().mesh = (new ObjImporter ()).ImportFile (selectedFile);
					base.MasterController().GetComponent<scr_SceneController>().rawMesh = (new ObjImporter ()).GetRawMeshData (selectedFile);
				}
			}
			// Remove Model
			if (GUI.Button (new Rect (166, 65, 58, 24), "Reset")) {
				string content = defaultModel.text;
				GO_model.GetComponent<MeshFilter> ().mesh = (new ObjImporter ()).ImportFileRaw (content);
				base.MasterController().GetComponent<scr_SceneController>().rawMesh = content;
			}
			// Select a Texture
			if (GUI.Button (new Rect (32, 94, 128, 24), "Select texture")) {
				string selectedFile = FileBrowser.OpenSingleFile ("Select texture", base.MasterController().GetComponent<scr_SceneController>().fp_texture, "jpg,jpeg,png");
				if (!string.IsNullOrEmpty (selectedFile)) {
					base.MasterController().GetComponent<scr_SceneController>().fp_texture = selectedFile;
					base.MasterController().GetComponent<scr_SceneController>().UpdateFilePaths ();
					byte[] fileData = File.ReadAllBytes (selectedFile);
					Texture2D tex = new Texture2D (2, 2);
					tex.LoadImage (fileData);
					tex.Apply ();
					GO_model.GetComponent<MeshRenderer> ().material.mainTexture = tex;
					base.MasterController().GetComponent<scr_SceneController>().dim_tex_x [0] = tex.width;
					base.MasterController().GetComponent<scr_SceneController>().dim_tex_y [0] = tex.height;
					base.MasterController().GetComponent<scr_SceneController>().texFormat [0] = tex.format.ToString ();
					base.MasterController().GetComponent<scr_SceneController>().mipMap [0] = tex.mipmapCount;
				}
			}
			// Remove Texture
			if (GUI.Button (new Rect (166, 94, 58, 24), "Reset")) {
				Texture2D tex = new Texture2D (2, 2);
				tex.Apply ();
				GO_model.GetComponent<MeshRenderer> ().material.mainTexture = tex;
				base.MasterController().GetComponent<scr_SceneController>().dim_tex_x [0] = 1;
				base.MasterController().GetComponent<scr_SceneController>().dim_tex_y [0] = 1;
				base.MasterController().GetComponent<scr_SceneController>().texFormat [0] = "";
				base.MasterController().GetComponent<scr_SceneController>().mipMap [0] = 0;
			}
			// Toggle Normal
			bool new_shader_bump = GUI.Toggle (new Rect(10, 123, 24, 24), base.MasterController().GetComponent<scr_SceneController>().shader_bump, "");
			if (new_shader_bump != base.MasterController().GetComponent<scr_SceneController>().shader_bump) {
				base.MasterController().GetComponent<scr_SceneController>().shader_bump = new_shader_bump;
				base.MasterController().GetComponent<scr_SceneController>().SetModelShader ();
			}
			// Select a Normal Map
			if (GUI.Button (new Rect (32, 123, 128, 24), "Select normal map")) {
				string selectedFile = FileBrowser.OpenSingleFile ("Select normal map", base.MasterController().GetComponent<scr_SceneController>().fp_normal, "jpg,jpeg,png");
				if (!string.IsNullOrEmpty (selectedFile)) {
					base.MasterController().GetComponent<scr_SceneController>().fp_normal = selectedFile;
					base.MasterController().GetComponent<scr_SceneController>().UpdateFilePaths ();
					byte[] fileData = File.ReadAllBytes (selectedFile);
					Texture2D tex = new Texture2D (2, 2);
					tex.LoadImage (fileData);
					tex.Apply ();
					GO_model.GetComponent<MeshRenderer> ().material.EnableKeyword ("_NORMALMAP");
					GO_model.GetComponent<MeshRenderer> ().material.SetTexture ("_BumpMap", tex);
					base.MasterController().GetComponent<scr_SceneController>().dim_tex_x [1] = tex.width;
					base.MasterController().GetComponent<scr_SceneController>().dim_tex_y [1] = tex.height;
					base.MasterController().GetComponent<scr_SceneController>().texFormat [1] = tex.format.ToString ();
					base.MasterController().GetComponent<scr_SceneController>().mipMap [1] = tex.mipmapCount;
				}
			}
			// Remove Normal
			if (GUI.Button (new Rect (166, 123, 58, 24), "Reset")) {
				Texture2D tex = new Texture2D (2, 2);
				tex.Apply ();
				GO_model.GetComponent<MeshRenderer> ().material.SetTexture ("_BumpMap", tex);
				base.MasterController().GetComponent<scr_SceneController>().dim_tex_x [1] = 1;
				base.MasterController().GetComponent<scr_SceneController>().dim_tex_y [1] = 1;
				base.MasterController().GetComponent<scr_SceneController>().texFormat [1] = "";
				base.MasterController().GetComponent<scr_SceneController>().mipMap [1] = 0;
			}
			// Toggle specularity
			bool new_shader_spec = GUI.Toggle (new Rect(10, 152, 24, 24), base.MasterController().GetComponent<scr_SceneController>().shader_spec, "");
			if (new_shader_spec != base.MasterController().GetComponent<scr_SceneController>().shader_spec) {
				base.MasterController().GetComponent<scr_SceneController>().shader_spec = new_shader_spec;
				base.MasterController().GetComponent<scr_SceneController>().SetModelShader ();
			}
			// Set specularity
			UpdateSpecularity();
			GUI.backgroundColor = Color.gray;
			// Toggle Transparency
			bool new_shader_tran = GUI.Toggle (new Rect(10, 181, 128, 24), base.MasterController().GetComponent<scr_SceneController>().shader_tran, "  Transparency");
			if (new_shader_tran != base.MasterController().GetComponent<scr_SceneController>().shader_tran) {
				base.MasterController().GetComponent<scr_SceneController>().shader_tran = new_shader_tran;
				base.MasterController().GetComponent<scr_SceneController>().SetModelShader ();
			}
			// Select a image marker
			if (GUI.Button (new Rect (32, 210, 128, 24), "Select marker")) {
				string selectedFile = FileBrowser.OpenSingleFile ("Select marker", base.MasterController().GetComponent<scr_SceneController>().fp_marker, "jpg,jpeg,png");
				if (!string.IsNullOrEmpty (selectedFile)) {
					base.MasterController().GetComponent<scr_SceneController>().fp_marker = selectedFile;
					base.MasterController().GetComponent<scr_SceneController>().UpdateFilePaths ();
					byte[] fileData = File.ReadAllBytes (selectedFile);
					Texture2D tex = new Texture2D (2, 2);
					tex.LoadImage (fileData);
					tex.Apply ();
					GO_marker.GetComponent<MeshRenderer> ().material.mainTexture = tex;
					GO_marker.GetComponent<scr_MarkerSizer> ().NormalizeScale (tex.width, tex.height);
					base.MasterController().GetComponent<scr_SceneController>().dim_tex_x [2] = tex.width;
					base.MasterController().GetComponent<scr_SceneController>().dim_tex_y [2] = tex.height;
					base.MasterController().GetComponent<scr_SceneController>().texFormat [2] = tex.format.ToString ();
					base.MasterController().GetComponent<scr_SceneController>().mipMap [2] = tex.mipmapCount;
				}
			}
			// Remove Marker
			if (GUI.Button (new Rect (166, 210, 58, 24), "Reset")) {
				Texture2D tex = new Texture2D (2, 2);
				tex.Apply ();
				GO_marker.GetComponent<MeshRenderer> ().material.mainTexture = tex;
				base.MasterController().GetComponent<scr_SceneController>().dim_tex_x [2] = 1;
				base.MasterController().GetComponent<scr_SceneController>().dim_tex_y [2] = 1;
				base.MasterController().GetComponent<scr_SceneController>().texFormat [2] = "";
				base.MasterController().GetComponent<scr_SceneController>().mipMap [2] = 0;
			}
			// Transformations
			{
				// Model Transform
				if (base.MasterController ().GetComponent<scr_SceneController> ().mouseState == scr_SceneController.MouseState.EDITING) {
					UpdateModel ();
				}
				// Camera Transform
				else if (base.MasterController ().GetComponent<scr_SceneController> ().mouseState == scr_SceneController.MouseState.CAMERA) {
					UpdateCamera ();
				}
				// Light Transform
				else if (base.MasterController ().GetComponent<scr_SceneController> ().mouseState == scr_SceneController.MouseState.LIGHT) {
					UpdateLight ();
				}
			}
			// Save changes
			GUI.backgroundColor = Color.gray;
			if (GUI.Button (new Rect (10, 255, 170, 32), "Save Changes")) {
				// Save changes to Web Server
				StartCoroutine (base.MasterController().GetComponent<scr_Save>().Activate ());
			}
			// Upload Vuforia Marker
			if (GUI.Button (new Rect (10, 290, 170, 32), "Upload Vuforia Marker")) {
				// Send the marker data to Vuforia
				StartCoroutine (base.MasterController().GetComponent<scr_Save>().UploadVuforiaMarker ());
			}
			// Preview
			if (GUI.Button (new Rect (10, 325, 170, 32), "Preview")) {
				base.MasterController().GetComponent<scr_SceneController>().mouseState = scr_SceneController.MouseState.LOCKED;
				base.MasterController().GetComponent<scr_SceneController>().ChangeState(scr_SceneController.ProgState.PREVIEW);
				GO_marker.GetComponent<MeshRenderer> ().enabled = false;
				GO_model.GetComponent<MeshRenderer> ().enabled = false;
				GO_camera.SetActive (false);
				GO_camera.GetComponent<Camera> ().enabled = false;
				AR_camera.SetActive (true);
				AR_camera.GetComponent<Camera> ().enabled = true;
				this.GetComponent<scr_Edit> ().ShowGUI (false);
				this.GetComponent<scr_Preview> ().ShowGUI (true);
				return;
			}
		}
		// Camera controls
		string mState = "";
		if (base.MasterController().GetComponent<scr_SceneController>().mouseState == scr_SceneController.MouseState.CAMERA)
			mState = "Camera";
		else if (base.MasterController().GetComponent<scr_SceneController>().mouseState == scr_SceneController.MouseState.LIGHT)
			mState = "Light";
		else if (base.MasterController().GetComponent<scr_SceneController>().mouseState == scr_SceneController.MouseState.EDITING)
			mState = "Editing";
		GUI.Label (new Rect (Screen.width - 180, 10, 200, 22), "Transform Mode: " + mState);
		if (GUI.Button (new Rect (Screen.width - 225, 30, 70, 24), "Editing")) {
			base.MasterController().GetComponent<scr_SceneController>().mouseState = scr_SceneController.MouseState.EDITING;
		}
		if (GUI.Button (new Rect (Screen.width - 150, 30, 70, 24), "Camera")) {
			base.MasterController().GetComponent<scr_SceneController>().mouseState = scr_SceneController.MouseState.CAMERA;
		}
		if (GUI.Button (new Rect (Screen.width - 75, 30, 70, 24), "Light")) {
			base.MasterController().GetComponent<scr_SceneController>().mouseState = scr_SceneController.MouseState.LIGHT;
		}

	}

	private void UpdateSpecularity() {
		// Specularity
		{
			{
				// Input
				GUI.Label (new Rect (32, 152, 80, 24), "Specularity:");
				{
					// Intensity
					try {
						float.Parse (tSpecularity);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tSpecularity = GUI.TextField (new Rect (110, 152, 88, 24), tSpecularity, 24);
				}
			}
			// Update
			{
				try {
					if (!base.MasterController ().GetComponent<scr_SceneController> ().specularity.Equals (tSpecularity) ){
						// Normalize Input
						{
							float amount = float.Parse(tSpecularity);
							if (amount < 0.1f) {
								tSpecularity = "0.1";
							}
							else if (amount > 1.0f) {
								tSpecularity = "1.0";
							}
						}
						GO_model.GetComponent<MeshRenderer>().material.SetFloat("_Shininess", float.Parse(tSpecularity));
						base.MasterController ().GetComponent<scr_SceneController> ().specularity = tSpecularity;
					}
				} catch (Exception e) {
					GO_model.GetComponent<MeshRenderer>().material.SetFloat("_Shininess", float.Parse(base.MasterController ().GetComponent<scr_SceneController> ().specularity));
				}
			}
		}
	}

	private void UpdateModel() {
		// Tranforms
		{
			{
				// Input
				GUI.Label (new Rect (Screen.width - 225,  60, 120, 20), "Model Transform:");
				GUI.Label (new Rect (Screen.width - 225,  85,  16, 20), "T");
				GUI.Label (new Rect (Screen.width - 225, 108,  16, 20), "R");
				GUI.Label (new Rect (Screen.width - 225, 131,  16, 20), "S");
				{
					// Translation X
					try {
						float.Parse (tx_mod [0]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [0] = GUI.TextField (new Rect (Screen.width - 205, 85, 64, 20), tx_mod [0], 32);
				}
				{
					// Translation Y
					try {
						float.Parse (tx_mod [1]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [1] = GUI.TextField (new Rect (Screen.width - 140, 85, 64, 20), tx_mod [1], 32);
				}
				{
					// Translation Z
					try {
						float.Parse (tx_mod [2]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [2] = GUI.TextField (new Rect (Screen.width -  75, 85, 64, 20), tx_mod [2], 32);
				}
				{
					// Rotation X
					try {
						float.Parse (tx_mod [3]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [3] = GUI.TextField (new Rect (Screen.width - 205, 108, 64, 20), tx_mod [3], 32);
				}
				{
					// Rotation Y
					try {
						float.Parse (tx_mod [4]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [4] = GUI.TextField (new Rect (Screen.width - 140, 108, 64, 20), tx_mod [4], 32);
				}
				{
					// Rotation Z
					try {
						float.Parse (tx_mod [5]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [5] = GUI.TextField (new Rect (Screen.width -  75, 108, 64, 20), tx_mod [5], 32);
				}
				{
					// Scale X
					try {
						float.Parse (tx_mod [6]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [6] = GUI.TextField (new Rect (Screen.width - 205, 131, 64, 20), tx_mod [6], 32);
				}
				{
					// Scale Y
					try {
						float.Parse (tx_mod [7]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [7] = GUI.TextField (new Rect (Screen.width - 140, 131, 64, 20), tx_mod [7], 32);
				}
				{
					// Scale Z
					try {
						float.Parse (tx_mod [8]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_mod [8] = GUI.TextField (new Rect (Screen.width -  75, 131, 64, 20), tx_mod [8], 32);
				}
			}
			{
				// Update
				{
					// Transform
					try {
						if (!base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [0].Equals (tx_mod [0]) ||
						   !base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [1].Equals (tx_mod [1]) ||
						   !base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [2].Equals (tx_mod [2])) {
							GO_model.transform.localPosition = new Vector3 (float.Parse (tx_mod [0]), float.Parse (tx_mod [1]), float.Parse (tx_mod [2]));
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [0] = tx_mod [0];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [1] = tx_mod [1];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [2] = tx_mod [2];
						}
					} catch (Exception e) {
						GO_model.transform.localPosition = new Vector3 (
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [0]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [1]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [2])
						);
					}
				}
				{
					// Rotation
					try {
						if (!base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [3].Equals (tx_mod [3]) ||
							!base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [4].Equals (tx_mod [4]) ||
							!base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [5].Equals (tx_mod [5])) {
							GO_model.transform.eulerAngles = new Vector3 (float.Parse (tx_mod [3]), float.Parse (tx_mod [4]), float.Parse (tx_mod [5]));
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [3] = tx_mod [3];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [4] = tx_mod [4];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [5] = tx_mod [5];
						}
					} catch (Exception e) {
						GO_model.transform.eulerAngles = new Vector3 (
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [3]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [4]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [5])
						);
					}
				}
				{
					// Scale
					try {
						if (!base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [6].Equals (tx_mod [6]) ||
							!base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [7].Equals (tx_mod [7]) ||
							!base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [8].Equals (tx_mod [8])) {
							GO_model.transform.localScale = new Vector3 (float.Parse (tx_mod [6]), float.Parse (tx_mod [7]), float.Parse (tx_mod [8]));
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [6] = tx_mod [6];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [7] = tx_mod [7];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [8] = tx_mod [8];
						}
					} catch (Exception e) {
						GO_model.transform.localScale = new Vector3 (
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [6]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [7]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [8])
						);
					}
				}
			}
		}
	}

	private void UpdateCamera() {
		if (GUI.Button (new Rect (Screen.width - 225, 154, 220, 20), "Reset Camera")) {
			base.MasterController ().GetComponent<scr_SceneController> ().ResetCamera ();
		}

		// Tranforms
		{
			{
				// Input
				GUI.Label (new Rect (Screen.width - 225,  60, 120, 20), "Camera Transform:");
				GUI.Label (new Rect (Screen.width - 225,  85,  16, 20), "T");
				GUI.Label (new Rect (Screen.width - 225, 108,  16, 20), "R");
				{
					// Translation X
					try {
						float.Parse (tx_cam [0]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_cam [0] = GUI.TextField (new Rect (Screen.width - 205, 85, 64, 20), tx_cam [0], 32);
				}
				{
					// Translation Y
					try {
						float.Parse (tx_cam [1]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_cam [1] = GUI.TextField (new Rect (Screen.width - 140, 85, 64, 20), tx_cam [1], 32);
				}
				{
					// Translation Z
					try {
						float.Parse (tx_cam [2]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_cam [2] = GUI.TextField (new Rect (Screen.width -  75, 85, 64, 20), tx_cam [2], 32);
				}
				{
					// Rotation X
					try {
						float.Parse (tx_cam [3]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_cam [3] = GUI.TextField (new Rect (Screen.width - 205, 108, 64, 20), tx_cam [3], 32);
				}
				{
					// Rotation Y
					try {
						float.Parse (tx_cam [4]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_cam [4] = GUI.TextField (new Rect (Screen.width - 140, 108, 64, 20), tx_cam [4], 32);
				}
				{
					// Rotation Z
					try {
						float.Parse (tx_cam [5]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_cam [5] = GUI.TextField (new Rect (Screen.width -  75, 108, 64, 20), tx_cam [5], 32);
				}
			}
			{
				// Update
				{
					// Transform
					try {
						if (!base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [0].Equals (tx_cam [0]) ||
							!base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [1].Equals (tx_cam [1]) ||
							!base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [2].Equals (tx_cam [2])) {
							GO_camera.transform.localPosition = new Vector3 (float.Parse (tx_cam [0]), float.Parse (tx_cam [1]), float.Parse (tx_cam [2]));
							base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [0] = tx_cam [0];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [1] = tx_cam [1];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [2] = tx_cam [2];
						}
					} catch (Exception e) {
						GO_camera.transform.localPosition = new Vector3 (
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [0]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [1]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_mod [2])
						);
					}
				}
				{
					// Rotation
					try {
						if (!base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [3].Equals (tx_cam [3]) ||
							!base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [4].Equals (tx_cam [4]) ||
							!base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [5].Equals (tx_cam [5])) {
							GO_camera.transform.eulerAngles = new Vector3 (float.Parse (tx_cam [3]), float.Parse (tx_cam [4]), float.Parse (tx_cam [5]));
							base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [3] = tx_cam [3];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [4] = tx_cam [4];
							base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [5] = tx_cam [5];
						}
					} catch (Exception e) {
						GO_camera.transform.eulerAngles = new Vector3 (
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [3]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [4]), 
							float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_cam [5])
						);
					}
				}
			}
		}
	}

	private void UpdateLight() {
		// Tranforms
		{
			{
				// Input
				GUI.Label (new Rect (Screen.width - 225, 60, 220, 20), "Directional Light Transform:");
				GUI.Label (new Rect (Screen.width - 225, 108, 16, 20), "R");
				{
					// Rotation X
					try {
						float.Parse (tx_lit [3]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_lit [3] = GUI.TextField (new Rect (Screen.width - 205, 108, 64, 20), tx_lit [3], 32);
				}
				{
					// Rotation Y
					try {
						float.Parse (tx_lit [4]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tx_lit [4] = GUI.TextField (new Rect (Screen.width - 140, 108, 64, 20), tx_lit [4], 32);
				}
			}
			{
				// Update
				try {
					if (!base.MasterController ().GetComponent<scr_SceneController> ().txform_lit [3].Equals (tx_lit [3]) ||
					    !base.MasterController ().GetComponent<scr_SceneController> ().txform_lit [4].Equals (tx_lit [4]) ){
						GO_light.transform.eulerAngles = new Vector3 (float.Parse (tx_lit [3]), float.Parse (tx_lit [4]), 0.0f);
						base.MasterController ().GetComponent<scr_SceneController> ().txform_lit [3] = tx_lit [3];
						base.MasterController ().GetComponent<scr_SceneController> ().txform_lit [4] = tx_lit [4];
					}
				} catch (Exception e) {
					GO_light.transform.eulerAngles = new Vector3 (
						float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_lit [3]), 
						float.Parse (base.MasterController ().GetComponent<scr_SceneController> ().txform_lit [4]), 
						0.0f
					);
				}
			}
		}
		// Color
		{
			{
				// Input
				GUI.Label (new Rect (Screen.width - 225, 146, 120, 20), "RGB Color:");
				GUI.Label (new Rect (Screen.width - 225, 169, 16, 20), "R:");
				GUI.Label (new Rect (Screen.width - 160, 169, 16, 20), "G:");
				GUI.Label (new Rect (Screen.width - 95, 169, 16, 20), "B:");
				{
					// Red
					try {
						int.Parse (lite_col [0]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					lite_col [0] = GUI.TextField (new Rect (Screen.width - 205, 169, 32, 20), lite_col [0], 32);
				}
				{
					// Blue
					try {
						int.Parse (lite_col [1]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					lite_col [1] = GUI.TextField (new Rect (Screen.width - 140, 169, 32, 20), lite_col [1], 32);
				}
				{
					// Green
					try {
						int.Parse (lite_col [2]);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					lite_col [2] = GUI.TextField (new Rect (Screen.width - 75, 169, 32, 20), lite_col [2], 32);
				}
			}
			// Update
			{
				try {
					if (!base.MasterController ().GetComponent<scr_SceneController> ().color_lite [0].Equals (lite_col [0]) ||
						!base.MasterController ().GetComponent<scr_SceneController> ().color_lite [1].Equals (lite_col [1]) ||
						!base.MasterController ().GetComponent<scr_SceneController> ().color_lite [2].Equals (lite_col [2]) ){
						// Normalize Input
						{
							int r = int.Parse(lite_col [0]);
							int g = int.Parse(lite_col [1]);
							int b = int.Parse(lite_col [2]);
							if (lite_col [0].Length > 3) {
								lite_col [0] = lite_col [0].Substring(0, 3);
							}
							if (lite_col [1].Length > 3) {
								lite_col [1] = lite_col [1].Substring(0, 3);
							}
							if (lite_col [2].Length > 3) {
								lite_col [2] = lite_col [2].Substring(0, 3);
							}
							if (r < 0) {
								r = 0;
								lite_col [0] = "0";
							}
							else if (r > 255) {
								r = 255;
								lite_col [0] = "255";
							}
							if (g < 0) {
								g = 0;
								lite_col [1] = "0";
							}
							else if (g > 255) {
								g = 255;
								lite_col [1] = "255";
							}
							if (b < 0) {
								b = 0;
								lite_col [2] = "0";
							}
							else if (b > 255) {
								b = 255;
								lite_col [2] = "255";
							}
						}
						GO_light.GetComponent<Light>().color = new Color (
							(1.0f * int.Parse (lite_col [0])) / 255.0f,
							(1.0f * int.Parse (lite_col [1])) / 255.0f,
							(1.0f * int.Parse (lite_col [2])) / 255.0f
						);
						base.MasterController ().GetComponent<scr_SceneController> ().color_lite [0] = lite_col [0];
						base.MasterController ().GetComponent<scr_SceneController> ().color_lite [1] = lite_col [1];
						base.MasterController ().GetComponent<scr_SceneController> ().color_lite [2] = lite_col [2];
					}
				} catch (Exception e) {
					GO_light.GetComponent<Light>().color = new Color (
						(1.0f * int.Parse (base.MasterController ().GetComponent<scr_SceneController> ().color_lite [0])) / 255.0f,
						(1.0f * int.Parse (base.MasterController ().GetComponent<scr_SceneController> ().color_lite [1])) / 255.0f,
						(1.0f * int.Parse (base.MasterController ().GetComponent<scr_SceneController> ().color_lite [2])) / 255.0f
					);
				}
			}
		}
		// Intensity
		{
			{
				// Input
				GUI.Label (new Rect (Screen.width - 225, 202, 120, 20), "Intensity:");
				{
					// Intensity
					try {
						float.Parse (tIntensity);
						GUI.backgroundColor = Color.gray;
					} catch (Exception e) {
						GUI.backgroundColor = Color.red;
					}
					tIntensity = GUI.TextField (new Rect (Screen.width - 160, 202, 84, 20), tIntensity, 32);
				}
			}
			// Update
			{
				try {
					if (!base.MasterController ().GetComponent<scr_SceneController> ().intensity.Equals (tIntensity) ){
						// Normalize Input
						{
							float amount = float.Parse(tIntensity);
							if (amount < 0f) {
								amount = 0f;
								tIntensity = "0";
							}
						}
						GO_light.GetComponent<Light>().intensity = float.Parse(tIntensity);
						base.MasterController ().GetComponent<scr_SceneController> ().intensity = tIntensity;
					}
				} catch (Exception e) {
					GO_light.GetComponent<Light>().intensity = float.Parse(base.MasterController ().GetComponent<scr_SceneController> ().intensity);
				}
			}
		}
	}

}
