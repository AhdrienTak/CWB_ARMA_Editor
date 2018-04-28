using System;
using System.Net;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Crosstales.FB;


public class scr_SceneController : MonoBehaviour {

	// This list is based on illegal file name characters
	public readonly string[] INVALID_CHAR = new string[] {
		"~", "#", "%", "&", "*", "{", "}", "\\", ":", "<", ">", "?", "/", "+", "|", "\"", // This list is based on illegal file name characters
		".", ",", "^"                                                                     // This list is additional chracters we won't allow
	}; 
	// This list is characters we do not allow for passwords
	public readonly string[] INVALID_PASS = new string[] {
		"`", "~", "|", "\\", "\"", "'"                            
	}; 

	[SerializeField] public  readonly string BASE_URL        = "mergingspaces.net/";
	[SerializeField] private readonly string WEB_SERVER_USER = "mergingspaces@rachelclarke.net";
	[SerializeField] private readonly string WEB_SERVER_PASS = "Merging1";

	[SerializeField] private GameObject MS_gui;
	[SerializeField] private GameObject IT_target;
	[SerializeField] private GameObject GO_model;
	[SerializeField] private GameObject GO_marker;
	[SerializeField] private GameObject GO_camera;
	[SerializeField] private GameObject AR_camera;
	[SerializeField] private GameObject GO_light;
	[SerializeField] private TextAsset defaultModel;


	public  string fp_object;   // Remembers the last location the user navigated to for their object     file
	public  string fp_texture;  // Remembers the last location the user navigated to for their texture    file
	public  string fp_normal;   // Remembers the last location the user navigated to for their normal map file
	public  string fp_marker;   // Remembers the last location the user navigated to for their marker     file

	private ProgState currState;
	private ProgState prevState;
	public  enum ProgState {
		LOGIN, GALLERY, PIECE, EDIT, PREVIEW, DELETE
	}

	public  MouseState mouseState;
	public  enum MouseState {
		EDITING, CAMERA, LIGHT, LOCKED
	}

	private bool         lock_changingStates;
	private bool         lock_authentication;
	private bool         lock_refreshingLists;
	private bool         lock_creatingDirectory;
	private bool         lock_deletingDirectory;
	private bool         lock_lockingDirectory;
	private bool         lock_openingDirectory;
	private bool[]       lock_uploadingData;
	private bool[]       lock_downloadingData;
	private bool         lock_savingEdits;

	public  List<string> listings;       // A list of all pertinent directories (Galleries or pieces)
	public  string       folderType;     // "Gallery" or "Piece"
	public  string       currentGallery; // The name of the gallery folder the user has selected
	public  string       currentPiece;   // The name of the piece   folder the user has selected
	public  string       liveGallery;    // Keeps track of a gallery's public visibility
	public  string       livePiece;      // Keeps track of a piece's public visibility

	public  int          indexCur;    // The current directory at the top of the scroll list
	public  int          indexMax;    // The maximum amount of directories to be visible on screen at once

	public  bool         showEditGUI; // Hides the Editing GUI to make viewing the model easier

	private string       uName; // User's name
	private string       uPass; // Password
	public  string       uMess; // Various messages
	public  string       fName; // File Name

	private string       debugMsg;

	public  string       markerID;
	public  string[]     txform_mod;
	public  string[]     txform_cam;
	public  string[]     txform_lit;
	public  string[]     color_lite;
	public  string       intensity;
	public  string       rawMesh;
	public  int[]        dim_tex_x;
	public  int[]        dim_tex_y;
	public  string[]     texFormat;
	public  string       specularity;
	public  int[]        mipMap;

	public  bool shader_bump;
	public  bool shader_spec;
	public  bool shader_tran;


	void Awake () {
		// Initialize all of the locks to "off"
		lock_changingStates    = false;
		lock_authentication    = false;
		lock_refreshingLists   = false;
		lock_creatingDirectory = false;
		lock_deletingDirectory = false;
		lock_lockingDirectory  = false;
		lock_openingDirectory  = false;
		lock_uploadingData     = new bool[] {false, false, false, false, false, false}; // Obj, Transform, Texture, Normal, Marker
		lock_downloadingData   = new bool[] {false, false, false, false, false, false}; // Obj, Transform, Texture, Normal, Marker
		lock_savingEdits       = false;

		// Initialize navigation integers
		indexCur = 0;
		indexMax = 5;

		// Initialize editing variables
		showEditGUI = true;

		// Initialize the file browsing variables
		listings = new List<string> ();
		folderType = "";
		currentGallery = "";

		// Initialize the system tracking variables
		uName = "";
		uPass = "";
		uMess = "";
		fName = "";

		// Initialize the debugging message
		debugMsg = "Program Launched";

		// Initialize the transformers
		txform_mod = new string[] {
			"0.0", "0.0", "0.0", // Translations
			"0.0", "0.0", "0.0", // Rotations
			"1.0", "1.0", "1.0"  // Scaling
		};
		txform_cam = new string[] {
			"0.0", "0.0", "0.0", // Translations
			"0.0", "0.0", "0.0", // Rotations
			"1.0", "1.0", "1.0"  // Scaling
		};
		txform_lit = new string[] {
			"0.0", "0.0", "0.0", // Translations
			"0.0", "0.0", "0.0", // Rotations
			"1.0", "1.0", "1.0"  // Scaling
		};
		color_lite = new string[] {
			"255", "255", "255"
		};
		intensity   = "";
		rawMesh     = "";
		dim_tex_x   = new int   [] { 0,  0,  0};
		dim_tex_y   = new int   [] { 0,  0,  0};
		texFormat   = new string[] {"", "", ""};
		mipMap      = new int   [] { 0,  0,  0};
		specularity = "0.1";
		markerID    = "";

		// Make the models and marker invisible at the start
		GO_model.GetComponent<MeshRenderer> ().enabled = false;
		GO_marker.GetComponent<MeshRenderer> ().enabled = false;
	}

	void Start() {
		// Delete everything in the temp folder
		DebugMessage("Deleting temporary files...");
		DirectoryInfo dirInfo = new DirectoryInfo (Application.persistentDataPath + "/Temp/");
		if (!dirInfo.Exists) {
			dirInfo.Create ();
		}
		try {
			foreach (string file in System.IO.Directory.GetFiles(Application.persistentDataPath + "/Temp/")) { 
				File.Delete (file);
			}
		} catch (Exception e) {
			DebugMessage (e.Message, "error");
		}

		GO_camera.SetActive (true);
		AR_camera.SetActive (false);
		LoadFilePaths ();

		// Notify the system that the user has entered the Login screen
		currState = ProgState.LOGIN;
		prevState = ProgState.LOGIN;
		ChangeState (ProgState.LOGIN);

		// Initialize the mouse state
		mouseState = MouseState.EDITING;
	}

	void Update() {
		// If any of the lock states are active, do not allow the user to move the camera
		if (CheckLocks()){
			return;
		}

		if (currState == ProgState.EDIT) {

			// Translations
			if (mouseState == MouseState.CAMERA) {
				// Translate Left and Right
				if (Input.GetMouseButton (0) && Input.GetAxis ("Mouse X") < 0) {
					GO_camera.GetComponent<scr_CameraControl> ().Translate ("L");
				} else if (Input.GetMouseButton (0) && Input.GetAxis ("Mouse X") > 0) {
					GO_camera.GetComponent<scr_CameraControl> ().Translate ("R");
				}
				// Translate Up and Down
				if (Input.GetMouseButton (0) && Input.GetAxis ("Mouse Y") < 0) {
					GO_camera.GetComponent<scr_CameraControl> ().Translate ("D");
				} else if (Input.GetMouseButton (0) && Input.GetAxis ("Mouse Y") > 0) {
					GO_camera.GetComponent<scr_CameraControl> ().Translate ("U");
				}
				// Translate In and Out
				if (Input.GetMouseButton (1) && Input.GetAxis ("Mouse Y") < 0) {
					GO_camera.GetComponent<scr_CameraControl> ().Translate ("O");
				} else if (Input.GetMouseButton (1) && Input.GetAxis ("Mouse Y") > 0) {
					GO_camera.GetComponent<scr_CameraControl> ().Translate ("I");
				} else if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
					GO_camera.GetComponent<scr_CameraControl> ().Translate ("I");
				} else if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
					GO_camera.GetComponent<scr_CameraControl> ().Translate ("O");
				}
			}

			// Rotations
			else if (mouseState == MouseState.LIGHT) {
				if (Input.GetMouseButton (0)) {
					GO_camera.GetComponent<scr_CameraControl> ().Rotate (Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
				}
			}

		}

	}

	void OnGUI() {
		// Always show the debugging line
		GUI.Label (new Rect (4, Screen.height - 22, Screen.width - 8, 22), debugMsg);
	}

	public void ChangeState(ProgState p) {
		lock_changingStates = true;

		HideGUIs ();

		// Figure out the center of the screen
		float x0 = Screen.width  / 2.0f;
		float y0 = Screen.height / 2.0f;
		int current;
		int offset;

		prevState = currState;
		currState = p;

		switch (currState) {

		case ProgState.LOGIN:
			uName = "";
			uPass = "";
			MS_gui.GetComponent<scr_Login> ().PassVars (x0, y0, uName, uPass);
			MS_gui.GetComponent<scr_Login> ().ShowGUI (true);
			break;

		case ProgState.GALLERY:
			indexCur = 0;
			folderType = "Gallery";
			fName = "New Gallery Name";
			currentGallery = "";
			MS_gui.GetComponent<scr_Gallery> ().PassVars (x0, y0, uName);
			MS_gui.GetComponent<scr_Gallery> ().ShowGUI (true);
			break;

		case ProgState.PIECE:
			indexCur = 0;
			folderType = "Piece";
			fName = "New Piece Name";
			currentPiece = "";
			rawMesh = "";
			for (int i = 0; i < dim_tex_x.Length; i++) {
				dim_tex_x [i] = 0;
				dim_tex_y [i] = 0;
				texFormat [i] = "";
				mipMap [i] = 0;
			}
			specularity = "0.1";
			GO_camera.GetComponent<scr_CameraControl> ().ResetCamera ();
			txform_mod = new string[] {
				"0.0", "0.0", "0.0", // Translations
				"0.0", "0.0", "0.0", // Rotations
				"1.0", "1.0", "1.0"  // Scaling
			};
			txform_cam = new string[] {
				GO_camera.transform.position.x + "",    GO_camera.transform.position.y + "",    GO_camera.transform.position.z + "", // Translations
				GO_camera.transform.eulerAngles.x + "", GO_camera.transform.eulerAngles.y + "", GO_camera.transform.eulerAngles.z + "", // Rotations
				"1.0", "1.0", "1.0"  // Scaling
			};
			txform_lit = new string[] {
				"0.0", "0.0", "0.0", // Translations
				GO_light.transform.eulerAngles.x + "", GO_light.transform.eulerAngles.y + "", "0.0", // Rotations
				"1.0", "1.0", "1.0"  // Scaling
			};
			color_lite = new string[] {
				"255", "255", "255"
			};
			intensity = "0.5";
			GO_light.GetComponent<Light> ().color = new Color (1.0f, 1.0f, 1.0f);
			GO_light.GetComponent<Light> ().intensity = 0.5f;
			shader_bump = false;
			shader_spec = false;
			shader_tran = false;
			MS_gui.GetComponent<scr_Piece> ().PassVars (x0, y0, uName);
			MS_gui.GetComponent<scr_Piece> ().ShowGUI (true);
			break;

		case ProgState.EDIT:
			showEditGUI = true;
			GO_model.GetComponent<MeshRenderer> ().enabled = true;
			GO_marker.GetComponent<MeshRenderer> ().enabled = true;
			mouseState = MouseState.EDITING;
			StartCoroutine(this.GetComponent<scr_Download>().Activate ());
			break;

		case ProgState.DELETE:
			string delType = "gallery";
			if (prevState == ProgState.PIECE)
				delType = "piece";
			uMess = "Delete " + delType + "?";
			MS_gui.GetComponent<scr_Deletion> ().PassVars (x0, y0, uName);
			MS_gui.GetComponent<scr_Deletion> ().ShowGUI (true);
			break;

		default:
			break;	
		}

		lock_changingStates = false;
	}

	private void HideGUIs() {
		MS_gui.GetComponent<scr_Login>    ().ShowGUI (false);
		MS_gui.GetComponent<scr_Gallery>  ().ShowGUI (false);
	    MS_gui.GetComponent<scr_Piece>    ().ShowGUI (false);
		MS_gui.GetComponent<scr_Deletion> ().ShowGUI (false);
		MS_gui.GetComponent<scr_Edit>     ().ShowGUI (false);
		MS_gui.GetComponent<scr_Preview>  ().ShowGUI (false);
	}

	public void SetModelShader() {
		GO_model.GetComponent<scr_MaterialSwap> ().setMaterial (shader_bump, shader_spec, shader_tran);
		GO_model.GetComponent<MeshRenderer> ().material.shader = Shader.Find (GO_model.GetComponent<scr_MaterialSwap> ().getMaterialString());
	}

	public void UpdateCamera() {
		txform_cam = new string[] {
			GO_camera.transform.position.x + "",    GO_camera.transform.position.y + "",    GO_camera.transform.position.z + "", // Translations
			GO_camera.transform.eulerAngles.x + "", GO_camera.transform.eulerAngles.y + "", GO_camera.transform.eulerAngles.z + "", // Rotations
			"1.0", "1.0", "1.0"  // Scaling
		};
	}

	public void ResetCamera() {
		GO_camera.GetComponent<scr_CameraControl> ().ResetCamera ();
		txform_cam = new string[] {
			GO_camera.transform.position.x + "",    GO_camera.transform.position.y + "",    GO_camera.transform.position.z + "", // Translations
			GO_camera.transform.eulerAngles.x + "", GO_camera.transform.eulerAngles.y + "", GO_camera.transform.eulerAngles.z + "", // Rotations
			"1.0", "1.0", "1.0"  // Scaling
		};
	}

	public void setLock(string lockName, bool b) {
		switch (lockName) {
		case "auth": lock_authentication    = b; break;
		case "cdir": lock_creatingDirectory = b; break;
		case "ddir": lock_deletingDirectory = b; break;
		case "odir": lock_openingDirectory  = b; break;
		case "ldir": lock_lockingDirectory  = b; break;
		case "list": lock_refreshingLists   = b; break;
		case "save": lock_savingEdits       = b; break;
		default:                                 break;
		}
	}

	public void setLock(string lockName, int lockNumber, bool b) {
		switch (lockName) {
		case "upld": lock_uploadingData[lockNumber]   = b; break;
		case "dnld": lock_downloadingData[lockNumber] = b; break;
		default:                                           break;
		}
	}

	public void DebugMessage(string message) {
		DebugMessage (message, "normal");
	}

	public void DebugMessage(string message, string msgType) {
		//TODO creeate a debug record that is scrollable
		debugMsg = message;
		msgType = msgType.ToLower ();
		switch (msgType) {
		case "error":
			Debug.LogError ("Error: " + message);
			break;
		case "assertion":
			Debug.LogAssertion (message);
			break;
		case "warning":
			Debug.LogWarning ("Warning: " + message);
			break;
		default:
			Debug.Log (message);
			break;
		}
	}

	private void LoadFilePaths() {
		// Get the last used filepaths
		try {
			// Save a default filepath folder
			if (!System.IO.File.Exists (Application.persistentDataPath + "/CWBARMA.dat")) {
				fp_object  = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
				fp_texture = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
				fp_normal  = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
				fp_marker  = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
				TextWriter tw = new StreamWriter (Application.persistentDataPath + "/CWBARMA.dat");
				tw.Write (
					fp_object  + "\n" +
					fp_texture + "\n" +
					fp_normal  + "\n" +
					fp_marker  + "\n"
				);
				tw.Close ();
			} 
			// Load the last filepaths used for searching
			else {
				TextReader tr = new StreamReader (Application.persistentDataPath + "/CWBARMA.dat");
				fp_object  = tr.ReadLine ();
				fp_texture = tr.ReadLine ();
				fp_normal  = tr.ReadLine ();
				fp_marker  = tr.ReadLine ();
				tr.Close ();
			}
		}
		// If it fails, just use defaults and forget about saving the dat file
		catch (Exception e) {
			DebugMessage ("Error: Could not create CWBARMA.dat file!", "error");
			fp_object  = "/";
			fp_texture = "/";
			fp_normal  = "/";
			fp_marker  = "/";
		}
	}

	public void UpdateFilePaths() {
		try {
			TextWriter tw = new StreamWriter (Application.persistentDataPath + "/CWBARMA.dat");
			tw.Write (
				fp_object  + "\n" +
				fp_texture + "\n" +
				fp_normal  + "\n" +
				fp_marker  + "\n"
			);
			tw.Close ();
		} 
		catch (Exception e) {
			DebugMessage ("Error: Could not create CWBARMA.dat", "error");
		}
	}

	public bool CheckLocks() {
		if (lock_authentication      ||
			lock_creatingDirectory   ||
			lock_openingDirectory    ||
			lock_deletingDirectory   ||
			lock_lockingDirectory    ||
			lock_refreshingLists     ||
			lock_uploadingData [0]   ||
			lock_uploadingData [1]   ||
			lock_uploadingData [2]   ||
			lock_uploadingData [3]   ||
			lock_uploadingData [4]   ||
			lock_uploadingData [5]   ||
			lock_downloadingData [0] ||
			lock_downloadingData [1] ||
			lock_downloadingData [2] ||
			lock_downloadingData [3] ||
			lock_downloadingData [4] ||
			lock_downloadingData [5] ||
			lock_savingEdits          )
			return true;
		else
			return false;
	}

	public string GetCurrentTargetName() {
		return "_" + uName + "_" + currentGallery + "_" + currentPiece;
	}

	public void SetMarkerID (string id) {
		DebugMessage ("Marker ID Created: " + id);
		markerID = id;
	}

	public string GetMarkerID () {
		return markerID;
	}

	public string GetUName() {
		return uName;
	}

	public void SetUName(string u) {
		uName = u;
	}

	public string GetUPass() {
		return uPass;
	}

	public ProgState GetCurrentProgState() {
		return currState;
	}

	public void SetCurrentProgState(ProgState ps) {
		currState = ps;
	}

	public ProgState GetPreviousProgState() {
		return prevState;
	}

	public string GetWSU() {
		return WEB_SERVER_USER;
	}

	public string GetWSP() {
		return WEB_SERVER_PASS;
	}

	public GameObject GetGOLight() {
		return GO_light;
	}

	public GameObject GetGOModel() {
		return GO_model;
	}

	public GameObject GetGOMarker() {
		return GO_marker;
	}

	public TextAsset GetDefaultModel() {
		return defaultModel;
	}

}
