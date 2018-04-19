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
	private readonly string[] INVALID_CHAR = new string[] {
		"~", "#", "%", "&", "*", "{", "}", "\\", ":", "<", ">", "?", "/", "+", "|", "\"", // This list is based on illegal file name characters
		".", ",", "^"                                                                     // This list is additional chracters we won't allow
	}; 
	// This list is characters we do not allow for passwords
	private readonly string[] INVALID_PASS = new string[] {
		"`", "~", "|", "\\", "\"", "'"                            
	}; 
		
	[SerializeField] // FILL THIS IN
	private readonly string BASE_URL = "mergingspaces.net/";
	[SerializeField] // FILL THIS IN
	private readonly string WEB_SERVER_USER = "mergingspaces@rachelclarke.net";
	[SerializeField] // FILL THIS IN
	private readonly string WEB_SERVER_PASS = "Merging1";

	[SerializeField]
	private GameObject IT_target;
	[SerializeField]
	private GameObject GO_model;
	[SerializeField]
	private GameObject GO_marker;
	[SerializeField]
	private GameObject GO_camera;
	[SerializeField]
	private GameObject AR_camera;

	private string fp_object;  // Remembers the last location the user navigated to for their object file
	private string fp_texture; // Remembers the last location the user navigated to for their texture file
	private string fp_marker;  // Remembers the last location the user navigated to for their marker file

	private ProgState currState;
	private ProgState prevState;
	private enum ProgState {
		LOGIN, GALLERY, PIECE, EDIT, PREVIEW, DELETE
	}

	private MouseState mouseState;
	private enum MouseState {
		EDITING, TRANSLATE, ROTATE, LOCKED
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

	private List<string> listings;       // A list of all pertinent directories (Galleries or pieces)
	private string       folderType;     // "Gallery" or "Piece"
	private string       currentGallery; // The name of the gallery folder the user has selected
	private string       currentPiece;   // The name of the piece   folder the user has selected
	private string       liveGallery;    // Keeps track of a gallery's public visibility
	private string       livePiece;      // Keeps track of a piece's public visibility

	private int          indexCur;    // The current directory at the top of the scroll list
	private int          indexMax;    // The maximum amount of directories to be visible on screen at once

	private bool         showEditGUI; // Hides the Editing GUI to make viewing the model easier

	private string       uName; // User's name
	private string       uPass; // Password
	private string       uMess; // Various messages
	private string       fName; // File Name

	private string       debugMsg;

	private string       markerID;
	private string[]     transforms;
	private string       rawMesh;
	private int[]        dim_tex_x;
	private int[]        dim_tex_y;
	private string[]     texFormat;
	private int[]        mipMap;


	void Awake () {
		// Delete everything in the temp folder
		DebugMessage("Deleting temporary files...");
		foreach (string file in System.IO.Directory.GetFiles(Application.dataPath + "/Resources/Temp/")) { 
			File.Delete (file);
		}

		// Notify the system that the user has entered the Login screen
		currState = ProgState.LOGIN;
		prevState = ProgState.LOGIN;

		// Initialize the mouse state
		mouseState = MouseState.EDITING;

		// Initialize all of the locks to "off"
		lock_changingStates    = false;
		lock_authentication    = false;
		lock_refreshingLists   = false;
		lock_creatingDirectory = false;
		lock_deletingDirectory = false;
		lock_lockingDirectory  = false;
		lock_openingDirectory  = false;
		lock_uploadingData     = new bool[] {false, false, false, false, false}; // Obj, Transform, Texture, Marker
		lock_downloadingData   = new bool[] {false, false, false, false, false}; // Obj, Transform, Texture, Marker
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
		transforms = new string[] {
			"0.0", "0.0", "0.0", // Translations
			"0.0", "0.0", "0.0", // Rotations
			"1.0", "1.0", "1.0"  // Scaling
		};
		rawMesh = "";
		dim_tex_x = new int[] {0, 0};
		dim_tex_y = new int[] {0, 0};
		texFormat = new string[] {"", ""};
		mipMap = new int[] {0, 0};
		markerID = "";

		// Make the models and marker invisible at the start
		GO_model.GetComponent<MeshRenderer> ().enabled = false;
		GO_marker.GetComponent<MeshRenderer> ().enabled = false;
	}
		
	void Start() {
		GO_camera.SetActive (true);
		AR_camera.SetActive (false);
		LoadFilePaths ();
	}

	void Update() {
		// If any of the lock states are active, do not allow the user to move the camera
		if (lock_authentication    ||
			lock_creatingDirectory ||
			lock_openingDirectory  ||
			lock_deletingDirectory ||
			lock_lockingDirectory  ||
			lock_refreshingLists   ||
			lock_uploadingData[0]  ||
			lock_uploadingData[1]  ||
			lock_uploadingData[2]  ||
			lock_uploadingData[3]  ||
			lock_uploadingData[4]  ||
			lock_downloadingData[0]||
			lock_downloadingData[1]||
			lock_downloadingData[2]||
			lock_downloadingData[3]||
			lock_downloadingData[4]||
			lock_savingEdits       ){
			return;
		}

		if (currState == ProgState.EDIT) {

			// Translations
			if (mouseState == MouseState.TRANSLATE) {
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
			else if (mouseState == MouseState.ROTATE) {
				if (Input.GetMouseButton (0)) {
					GO_camera.GetComponent<scr_CameraControl> ().Rotate (Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
				}
			}

		}

	}

	void OnGUI() {
		// Always show the debugging line
		GUI.Label (new Rect (4, Screen.height - 22, Screen.width - 8, 22), debugMsg);

		// If any of the lock states are active, do not allow the user to interact with the GUI
		if (lock_authentication    ||
			lock_creatingDirectory ||
			lock_openingDirectory  ||
			lock_deletingDirectory ||
			lock_lockingDirectory  ||
			lock_refreshingLists   ||
			lock_uploadingData[0]  ||
			lock_uploadingData[1]  ||
			lock_uploadingData[2]  ||
			lock_uploadingData[3]  ||
			lock_uploadingData[4]  ||
			lock_downloadingData[0]||
			lock_downloadingData[1]||
			lock_downloadingData[2]||
			lock_downloadingData[3]||
			lock_downloadingData[4]||
			lock_savingEdits       ){
			GUI.enabled = false;
		}
		else {
			GUI.enabled = true;
		}

		// Figure out the center of the screen
		float x0 = Screen.width  / 2.0f;
		float y0 = Screen.height / 2.0f;
		int current;
		int offset;

		// Determine the GUI state
		switch (currState) {


		// Login
		case ProgState.LOGIN:
			// Create Labels
			GUI.Label (new Rect (x0 - 256, y0 - 70, 256, 32), "Please Log In.");
			GUI.Label (new Rect (x0 - 200, y0 - 32, 128, 32), "User Name: ");
			GUI.Label (new Rect (x0 - 200, y0, 128, 32), " Password: ");
			GUI.Label (new Rect (x0 - 256, y0 + 64, 512, 32), uMess);
			// Create Text Fields
			uName = GUI.TextField (new Rect (x0 - 128, y0 - 32, 256, 32), uName, 32);
			uPass = GUI.PasswordField (new Rect (x0 - 128, y0, 256, 32), uPass, '*', 32);
			// Create Submit Button
			if (GUI.Button (new Rect (x0, y0 + 32, 64, 32), "Submit")) {
				StartCoroutine (VerifyCredentials ());
			}
			break;


		// Gallery
		case ProgState.GALLERY:
			//Create a Back button
			if (GUI.Button (new Rect (10, 10, 80, 32), "Back")) {
				ChangeState (ProgState.LOGIN);
				return;
			}
			// Create Top Buttons
			GUI.Label (new Rect (x0 - 100, 34, 256, 32), "Please select a gallery.");
			GUI.Label (new Rect (x0 + 100, 34, 256, 32), uMess);
			fName = GUI.TextField (new Rect (x0 - 30, 70, 256, 32), fName, 32);
			if (GUI.Button (new Rect (x0 - 240, 70, 200, 32), "Create new gallery")) {
				StartCoroutine (CreateDirectory ());
			}
			// Procedurally create buttons for each file
			current = 0;
			offset = 1;
			foreach (string item in listings) {
				// Ensure the correct buttons are visible on screen
				if (current <  indexCur)
					continue;
				if (current >= indexMax)
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
					StartCoroutine (LockDirectory (name, live));
				}
				GUI.backgroundColor = Color.gray;
				// Create the buttons for each Gallery
				if (GUI.Button (new Rect (x0 - 132, 70 + (offset * 36), 220, 32), name)) {
					liveGallery = live;
					StartCoroutine (OpenDirectory (name));
				}
				if (GUI.Button (new Rect (x0 + 104, 70 + (offset * 36), 92, 32), "Delete")) {
					StartCoroutine (DeleteDirectory (name));
				}
				offset++;
				current++;
			}
			//TODO create a scroll bar
			break;


		// Piece
		case ProgState.PIECE:
			//Create a Back button
			if (GUI.Button (new Rect (10, 10, 80, 32), "Back")) {
				ChangeState (ProgState.GALLERY);
				StartCoroutine (RefreshListings ());
				return;
			}
			// Create Top Buttons
			GUI.Label (new Rect (x0 - 100, 34, 256, 32), "Please select a piece.");
			GUI.Label (new Rect (x0 + 100, 34, 256, 32), uMess);
			fName = GUI.TextField (new Rect (x0 - 30, 70, 256, 32), fName, 32);
			if (GUI.Button (new Rect (x0 - 240, 70, 200, 32), "Create new piece")) {
				StartCoroutine (CreateDirectory ());
			}
			// Procedurally create buttons for each file
			current = 0;
			offset = 1;
			foreach (string item in listings) {
				// Ensure the correct buttons are visible on screen
				if (current <  indexCur)
					continue;
				if (current >= indexMax)
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
					StartCoroutine (LockDirectory (name, live));
				}
				GUI.backgroundColor = Color.gray;
				// Create the buttons for each Gallery
				if (GUI.Button (new Rect (x0 - 132, 70 + (offset * 36), 220, 32), name)) {
					livePiece = live;
					StartCoroutine (OpenDirectory (name));
				}
				if (GUI.Button (new Rect (x0 + 104, 70 + (offset * 36), 92, 32), "Delete")) {
					StartCoroutine (DeleteDirectory (name));
				}
				offset++;
				current++;
			}
			//TODO create a scroll bar
			break;


		// Edit
		case ProgState.EDIT:
			// Hide/Show GUI
			if (GUI.Button (new Rect (Screen.width - 130, 10, 120, 32), "Hide/Show GUI")) {
				showEditGUI = !showEditGUI;
			}
			if (showEditGUI) {
				// Create a Back button
				if (GUI.Button (new Rect (10, 10, 80, 32), "Back")) {
					ChangeState (ProgState.PIECE);
					GO_model.GetComponent<MeshRenderer> ().enabled = false;
					GO_marker.GetComponent<MeshRenderer> ().enabled = false;
					StartCoroutine (RefreshListings ());
					return;
				}
				// Show Editing buttons
				if (mouseState == MouseState.EDITING) {
					// Select a 3D model
					if (GUI.Button (new Rect (10, 80, 120, 32), "Select model")) {
						string selectedFile = FileBrowser.OpenSingleFile ("Select .obj", fp_object, "obj");
						if (!string.IsNullOrEmpty (selectedFile)) {
							fp_object = selectedFile;
							UpdateFilePaths ();
							GO_model.GetComponent<MeshFilter> ().mesh = (new ObjImporter ()).ImportFile (selectedFile);
							rawMesh = (new ObjImporter ()).GetRawMeshData (selectedFile);
						}
					}
					// Select a Texture
					if (GUI.Button (new Rect (10, 115, 120, 32), "Select texture")) {
						string selectedFile = FileBrowser.OpenSingleFile ("Select texture", fp_texture, "jpg,jpeg,png");
						if (!string.IsNullOrEmpty (selectedFile)) {
							fp_texture = selectedFile;
							UpdateFilePaths ();
							byte[] fileData = File.ReadAllBytes (selectedFile);
							Texture2D tex = new Texture2D (2, 2);
							tex.LoadImage (fileData);
							tex.Apply ();
							GO_model.GetComponent<MeshRenderer> ().material.mainTexture = tex;
							dim_tex_x [0] = tex.width;
							dim_tex_y [0] = tex.height;
							texFormat [0] = tex.format.ToString ();
							mipMap [0] = tex.mipmapCount;
						}
					}
					// Select a image marker
					if (GUI.Button (new Rect (10, 150, 120, 32), "Select marker")) {
						string selectedFile = FileBrowser.OpenSingleFile ("Select marker", fp_marker, "jpg,jpeg,png");
						if (!string.IsNullOrEmpty (selectedFile)) {
							fp_marker = selectedFile;
							UpdateFilePaths ();
							byte[] fileData = File.ReadAllBytes (selectedFile);
							Texture2D tex = new Texture2D (2, 2);
							tex.LoadImage (fileData);
							tex.Apply ();
							GO_marker.GetComponent<MeshRenderer> ().material.mainTexture = tex;
							GO_marker.GetComponent<scr_MarkerSizer> ().NormalizeScale (tex.width, tex.height);
							dim_tex_x [1] = tex.width;
							dim_tex_y [1] = tex.height;
							texFormat [1] = tex.format.ToString ();
							mipMap [1] = tex.mipmapCount;
						}
					}
					// Transformations
					{
						bool hasMoved = false;
						string[] old_transforms = new string[] {
							transforms [0], transforms [1], transforms [2], // Old Translations
							transforms [3], transforms [4], transforms [5], // Old Rotations
							transforms [6], transforms [7], transforms [8]  // Old Scalings
						};
						string[] new_transforms = new string[] {
							transforms [0], transforms [1], transforms [2], // New Translations
							transforms [3], transforms [4], transforms [5], // New Rotations
							transforms [6], transforms [7], transforms [8]  // New Scalings
						};
						// Translate
						GUI.Label (new Rect (40, 190, 120, 20), "Translate");
						new_transforms [0] = GUI.TextField (new Rect (10, 212, 64, 20), old_transforms [0], 32);
						new_transforms [1] = GUI.TextField (new Rect (80, 212, 64, 20), old_transforms [1], 32);
						new_transforms [2] = GUI.TextField (new Rect (150, 212, 64, 20), old_transforms [2], 32);
						// Rotate
						GUI.Label (new Rect (40, 234, 120, 20), "Rotate");
						new_transforms [3] = GUI.TextField (new Rect (10, 256, 64, 20), old_transforms [3], 32);
						new_transforms [4] = GUI.TextField (new Rect (80, 256, 64, 20), old_transforms [4], 32);
						new_transforms [5] = GUI.TextField (new Rect (150, 256, 64, 20), old_transforms [5], 32);
						// Scale
						GUI.Label (new Rect (40, 278, 120, 20), "Scale");
						new_transforms [6] = GUI.TextField (new Rect (10, 300, 64, 20), old_transforms [6], 32);
						new_transforms [7] = GUI.TextField (new Rect (80, 300, 64, 20), old_transforms [7], 32);
						new_transforms [8] = GUI.TextField (new Rect (150, 300, 64, 20), old_transforms [8], 32);
						// Check to make sure the inputs are valid, and save them
						try {
							for (int i = 0; i < 9; i++) {
								// Set to zero if the string is blank
								if (new_transforms [i].Equals ("")) {
									new_transforms [i] = "0";
								} else if (new_transforms [i].Equals ("-")) {
									new_transforms [i] = "-0";
								}
								// Check for validity, and if there has been a change
								if (double.Parse (transforms [i]) != double.Parse (new_transforms [i])) {
									hasMoved = true;
								}
								// Store it
								transforms [i] = new_transforms [i];
							}
						}
					// If there is an invalid input, put the old values back
					catch (FormatException e) {
							hasMoved = false;
							for (int i = 0; i < 9; i++) {
								transforms [i] = old_transforms [i];
							}
						}
						// Apply the transforms to the model
						if (hasMoved) {
							GO_model.GetComponent<scr_Transformer> ().setTransforms (transforms);
						}
					}
					// Save changes
					if (GUI.Button (new Rect ((Screen.width / 2) - 65, 10, 120, 32), "Save Changes")) {
						// Save changes to Web Server
						StartCoroutine (SaveEdits ());
					}
					// Preview
					if (GUI.Button (new Rect (Screen.width - 130, 85, 120, 32), "Preview")) {
						mouseState = MouseState.LOCKED;
						currState = ProgState.PREVIEW;
						GO_marker.GetComponent<MeshRenderer> ().enabled = false;
						GO_model.GetComponent<MeshRenderer> ().enabled = false;
						GO_camera.SetActive (false);
						GO_camera.GetComponent<Camera> ().enabled = false;
						AR_camera.SetActive (true);
						AR_camera.GetComponent<Camera> ().enabled = true;
						return;
					}
				}
				// Camera controls
				string mState = "";
				if (mouseState == MouseState.TRANSLATE)
					mState = "Translate";
				else if (mouseState == MouseState.ROTATE)
					mState = "Rotate";
				else if (mouseState == MouseState.EDITING)
					mState = "Editing";
				GUI.Label (new Rect (40, 330, 200, 22), "Camera Mode: " + mState);
				if (GUI.Button (new Rect (10, 354, 75, 24), "Editing")) {
					mouseState = MouseState.EDITING;
				}
				if (GUI.Button (new Rect (95, 354, 75, 24), "Translate")) {
					mouseState = MouseState.TRANSLATE;
				}
				if (GUI.Button (new Rect (180, 354, 75, 24), "Rotate")) {
					mouseState = MouseState.ROTATE;
				}
				if (GUI.Button (new Rect (10, 378, 200, 24), "Reset Camera")) {
					GO_camera.GetComponent<scr_CameraControl> ().ResetCamera ();
				}
			}
			break;

		
		// Preview
		case ProgState.PREVIEW:
			// Preview
			if (GUI.Button (new Rect (10, 10, 80, 32), "Finished")) {
				mouseState = MouseState.EDITING;
				currState = ProgState.EDIT;
				GO_marker.GetComponent<MeshRenderer> ().enabled = true;
				GO_model.GetComponent<MeshRenderer> ().enabled = true;
				IT_target.transform.localScale = new Vector3 (1f, 1f, 1f);
				GO_camera.SetActive (true);
				GO_camera.GetComponent<Camera> ().enabled = true;
				AR_camera.SetActive (false);
				AR_camera.GetComponent<Camera> ().enabled = false;
				return;
			}
			break;


		// Deletion
		case ProgState.DELETE:
			// Get folder type
			string type;
			if (prevState == ProgState.GALLERY)
				type = "Gallery";
			else if (prevState == ProgState.PIECE)
				type = "Piece";
			// Create Labels
			GUI.Label (new Rect (x0 - 256, y0 - 70, 512, 32), uMess);
			// Create buttons
			if (GUI.Button (new Rect (x0 - 50, y0, 64, 32), "No")) {
				StartCoroutine (DeleteDirectory (false));
			}
			if (GUI.Button (new Rect (x0 + 50, y0, 64, 32), "Yes")) {
				StartCoroutine (DeleteDirectory (true));
			}
			break;


		default:
			break;
		}
	}

	IEnumerator VerifyCredentials() {
		lock_authentication = true;

		// Handle empty inputs
		if (uName.Length < 1 || uPass.Length < 1) {
			DebugMessage ("Both username and password must be at least 1 character long.", "error");
			goto end_VerifyCredentials_B;
		}

		// Handle Invalid User Name
		for (int i = 0; i < INVALID_CHAR.Length; i++) {
			if (uName.Contains (INVALID_CHAR [i])) {
				DebugMessage ("Username contains illegal characters.", "error");
				goto end_VerifyCredentials_B;
			}
		}

		// Handle Invalid Password
		for (int i = 0; i < INVALID_PASS.Length; i++) {
			if (uPass.Contains (INVALID_PASS [i])) {
				DebugMessage ("Password contains illegal characters.", "error");
				goto end_VerifyCredentials_B;
			}
		}

		// Pull the login credentials data off of the web server
		UnityWebRequest www = UnityWebRequest.Get("http://" + BASE_URL + "Credentials/_" + uName.ToLower() + ".txt");
		www.chunkedTransfer = false;
		yield return www.SendWebRequest ();

		// Handle Network Error or File Error
		if(www.isNetworkError || www.isHttpError) {
			DebugMessage ("Unable to verify user: " + uName, "error");
			goto end_VerifyCredentials_A;
		}

		// Check Password
		if (www.downloadHandler.text.Equals (uPass)) {
			ChangeState (ProgState.GALLERY);
			DebugMessage ("Login Success!");
			uName = uName.ToLower ();
			yield return StartCoroutine(RefreshListings());
		}
		else {
			DebugMessage ("Unable to verify user: " + uName, "error");
		} 

		end_VerifyCredentials_A:
		www.Dispose ();
		end_VerifyCredentials_B:
		lock_authentication = false;
	}

	IEnumerator RefreshListings() {
		lock_refreshingLists = true;

		WWWForm form = new WWWForm();
		form.AddField("author", uName);
		form.AddField("type", folderType);
		form.AddField("gallery", currentGallery);
		UnityWebRequest www = UnityWebRequest.Post("http://" + BASE_URL + "Galleries/listDirectories.php", form);
		www.chunkedTransfer = false;
		yield return www.SendWebRequest ();

		// Handle Network Errors
		if(www.isNetworkError || www.isHttpError) {
			DebugMessage(www.error, "error");
			goto end_RefreshListings_A;
		}

		// Retrieve folders
		DebugMessage("Searching for " + folderType.ToLower() + " directories.");

		// Parse the downloaded contents
		listings = new List<string>();
		string foldersList = www.downloadHandler.text;

		if (foldersList.Equals ("failed")) {
			DebugMessage("Failed searching for " + folderType.ToLower() + " directories.", "error");
			goto end_RefreshListings_A;
		}

		while (foldersList.Length > 1) {
			int nextComma = foldersList.IndexOf (",");
			listings.Add (foldersList.Substring(0, nextComma));
			foldersList = foldersList.Substring (nextComma + 1);
		}

		DebugMessage ("Finished listing directories.");

		end_RefreshListings_A:
		www.Dispose ();
		end_RefreshListings_B:
		lock_refreshingLists = false;
	}

	IEnumerator CreateDirectory() {
		lock_creatingDirectory = true;

		// Check for illegal name
		if (fName.Length < 1) {
			DebugMessage("New " + folderType.ToLower() + "'s name must be at least 1 character long.", "error");
			goto end_CreateFile_B;
		}
		for (int i = 0; i < INVALID_CHAR.Length; i++) {
			if (fName.Contains (INVALID_CHAR [i])) {
				DebugMessage("New " + folderType.ToLower() + "'s name contains invalid characters.", "error");
				goto end_CreateFile_B;
			}
		}

		WWWForm form = new WWWForm();
		form.AddField("name", fName);
		form.AddField("type", folderType);
		form.AddField("author", uName);
		form.AddField("gallery", currentGallery);
		UnityWebRequest www = UnityWebRequest.Post("http://" + BASE_URL + "Galleries/createDirectory.php", form);
		www.chunkedTransfer = false;
		yield return www.SendWebRequest();

		// Handle Network Errors
		if(www.isNetworkError || www.isHttpError) {
			DebugMessage(www.error, "error");
			goto end_CreateFile_A;
		}

		// Handle File Creation
		DebugMessage("Directory created.");

		end_CreateFile_A:
		www.Dispose ();
		yield return StartCoroutine(RefreshListings());
		end_CreateFile_B:
		lock_creatingDirectory = false;
	}

	IEnumerator LockDirectory(string name, string lockState) {
		lock_lockingDirectory = true;

		WWWForm form = new WWWForm ();
		form.AddField ("author", uName);
		if (currState == ProgState.GALLERY) {
			form.AddField ("type", "Gallery");
			form.AddField ("gallery", name);
		} else {
			form.AddField ("type", "Piece");
			form.AddField ("gallery", currentGallery);
			form.AddField ("piece", name);
		}
		form.AddField ("lock", lockState);

		UnityWebRequest www = UnityWebRequest.Post ("http://" + BASE_URL + "Galleries/lockDirectory.php", form);
		www.chunkedTransfer = false;
		yield return www.SendWebRequest();

		// Handle Network Errors
		if (www.isNetworkError || www.isHttpError) {
			DebugMessage (www.error, "error");
		} 

		// Wait for deletion
		else {
			DebugMessage(www.downloadHandler.text);
		}

		www.Dispose ();
		yield return StartCoroutine(RefreshListings());

		lock_lockingDirectory = false;
	}

	IEnumerator OpenDirectory(string folderName) {
		lock_openingDirectory = true;
		if (currState == ProgState.GALLERY) {
			currentGallery = folderName;
			ChangeState (ProgState.PIECE);
		}
		else if (currState == ProgState.PIECE) {
			currentPiece = folderName;
			ChangeState (ProgState.EDIT);
		}
		yield return StartCoroutine(RefreshListings());
		lock_openingDirectory = false;
	}

	IEnumerator DeleteDirectory(string folderName) {
		lock_deletingDirectory = true;
		fName = folderName;
		ChangeState (ProgState.DELETE);
		yield return null;
		lock_deletingDirectory = false;
	}

	IEnumerator DeleteDirectory(bool affirmation) {
		lock_deletingDirectory = true;

		if (affirmation) {

			string type = "";
			if (prevState == ProgState.GALLERY)
				type = "Gallery";
			else if (prevState == ProgState.PIECE)
				type = "Piece";

			WWWForm form = new WWWForm ();
			form.AddField ("name", fName);
			form.AddField ("type", type);
			form.AddField ("author", uName);
			form.AddField ("gallery", currentGallery);
			UnityWebRequest www = UnityWebRequest.Post ("http://" + BASE_URL + "Galleries/deleteDirectory.php", form);
			www.chunkedTransfer = false;
			yield return www.SendWebRequest();

			// Handle Network Errors
			if (www.isNetworkError || www.isHttpError) {
				DebugMessage (www.error, "error");
			} 

			// Wait for deletion
			else {
				DebugMessage("Deleting Vuforia markers.");

				// Delete Vuforia cloud markers
				string markerIDs = www.downloadHandler.text;
				while (markerIDs.Length > 1) {
					string nextID = markerIDs.Substring (0, markerIDs.IndexOf (","));
					markerIDs = markerIDs.Substring (markerIDs.IndexOf(",") + 1);
					yield return StartCoroutine (this.GetComponent<CloudUploading>().CallDeleteTarget (nextID));
				}
			}

			www.Dispose ();
			yield return StartCoroutine(RefreshListings());

		}
		else {
			uMess = "Action canceled.";
			Debug.Log (uMess);
		}

		ChangeState (prevState);
		lock_deletingDirectory = false;
	}

	IEnumerator SaveEdits() {
		lock_savingEdits = true;

		// Save model 
		StartCoroutine(UploadFile("model", Encoding.UTF8.GetBytes (rawMesh), "model.obj"));

		// Save transform
		StartCoroutine (
			UploadFile (
				"transform", 
				Encoding.UTF8.GetBytes (
					transforms [0] + "," + transforms [1] + "," + transforms [2] + "," +
					transforms [3] + "," + transforms [4] + "," + transforms [5] + "," +
					transforms [6] + "," + transforms [7] + "," + transforms [8] + "," +
					" "
				), 
				"transform.txt"
			)
		);
			
		// Save image file dimensions
		string imageMetaData = dim_tex_x [0] + "," + // Width        of texture
		                       dim_tex_y [0] + "," + // Height       of texture 
		                       texFormat [0] + "," + // Format       of texture
		                       mipMap    [0] + "," + // MipMap count of texture
		                       dim_tex_x [1] + "," + // Width        of marker
		                       dim_tex_y [1] + "," + // Height       of marker
		                       texFormat [1] + "," + // Format       of marker
		                       mipMap    [1] + "," + // MipMap count of marker
		                       markerID      + "," + // Vuforia's ID of marker
		                       " ";
		
		StartCoroutine(UploadFile("dimension", Encoding.UTF8.GetBytes (imageMetaData), "dimension.txt"));

		// Upload image marker to Vuforia Cloud
		DebugMessage("Uploading image target to Vuforia cloud...");
		Texture2D imageTarget = (Texture2D) GO_marker.GetComponent<MeshRenderer> ().material.mainTexture;
		string markerName = this.GetCurrentTargetName() + "_marker";
		yield return StartCoroutine (this.GetComponent<CloudUploading>().CallPostTarget (imageTarget, markerName, imageMetaData));

		// Save texture
		StartCoroutine(UploadFile("texture", ((Texture2D) GO_model.GetComponent<MeshRenderer> ().material.mainTexture).GetRawTextureData(), "texture.png"));

		// Save marker
		StartCoroutine(UploadFile("marker", ((Texture2D) GO_marker.GetComponent<MeshRenderer> ().material.mainTexture).GetRawTextureData(), "marker.jpeg"));

		yield return null;
		lock_savingEdits = false;
	}

	IEnumerator UploadFile(string messageType, byte[] contents, string fileName) {
		string contentType = "";
		bool convertImage = false;
		string imageMime = "";

		switch (messageType) {
		case "model":
			lock_uploadingData [0] = true;
			break;
		case "transform":
			lock_uploadingData [1] = true;
			break;
		case "dimension":
			lock_uploadingData [2] = true;
			break;
		case "texture":
			lock_uploadingData [3] = true;
			convertImage = true;
			imageMime = "png";
			break;
		case "marker":
			lock_uploadingData [4] = true;
			convertImage = true;
			imageMime = "jpeg";
			break;
		default:
			break;
		}

		DebugMessage ("Uploading " + messageType + " data...");
		string filePath = "Galleries/_" + uName + "_" + currentGallery + liveGallery + "/_" + currentPiece + livePiece + "/" + fileName;
		FtpWebRequest wrq;
		FtpWebResponse wrp;
		Stream requestStream;

		try {
			wrq = (FtpWebRequest)WebRequest.Create ("ftp://" + BASE_URL + filePath);
			wrq.Method = WebRequestMethods.Ftp.UploadFile;
			wrq.Credentials = new NetworkCredential (WEB_SERVER_USER, WEB_SERVER_PASS);
			wrq.ContentLength = contents.Length;
			wrq.UseBinary = true;
			requestStream = wrq.GetRequestStream ();
			requestStream.Write (contents, 0, contents.Length);
			requestStream.Close ();
			wrp = (FtpWebResponse)wrq.GetResponse ();
			DebugMessage (wrp.StatusDescription);
			wrp.Close ();
			DebugMessage ("Uploading " + messageType + " data complete.");
		} catch (Exception e) {
			DebugMessage ("Uploading " + messageType + " data failed.", "error");
		}

		// If the target is an image, convert the byte array into a proper image file
		/*FIXME Was not able to get this to work...
		if (convertImage) {
			DebugMessage ("Attempting to decode image on web server...");
			WWWForm form = new WWWForm ();
			form.AddField ("mime",    imageMime     );
			form.AddField ("type",    messageType   );
			form.AddField ("author",  uName         );
			form.AddField ("gallery", currentGallery);
			form.AddField ("piece",   currentPiece  );
			UnityWebRequest www = UnityWebRequest.Post ("http://" + BASE_URL + "Galleries/decodeImage.php", form);
			www.chunkedTransfer = false;
			yield return www.SendWebRequest ();

			// Handle Errors
			if (www.isNetworkError) {
				DebugMessage (www.error, "error");
			} 

			// Wait for deletion
			else {
				DebugMessage ("Decoded image. " + www.downloadHandler.text);
			}

			www.Dispose ();
		}
		*/

		yield return null;
		switch (messageType) {
		case "model":
			lock_uploadingData [0] = false;
			break;
		case "transform":
			lock_uploadingData [1] = false;
			break;
		case "dimension":
			lock_uploadingData [2] = false;
			break;
		case "texture":
			lock_uploadingData [3] = false;
			break;
		case "marker":
			lock_uploadingData [4] = false;
			break;
		default:
			break;
		}
	}

	IEnumerator DownloadFile(string messageType, string fileName) {
		switch (messageType) {
		case "model":
			lock_downloadingData [0] = true;
			break;
		case "transform":
			lock_downloadingData [1] = true;
			break;
		case "dimension":
			lock_downloadingData [2] = true;
			break;
		case "texture":
			lock_downloadingData [3] = true;
			break;
		case "marker":
			lock_downloadingData [4] = true;
			break;
		default:
			break;
		}

		DebugMessage("Downloading " + messageType + " data...");
		string filePath = "Galleries/_" + uName + "_" + currentGallery + liveGallery + "/_" + currentPiece + livePiece + "/" + fileName;
		WebClient request = new WebClient ();
		request.Credentials = new NetworkCredential (WEB_SERVER_USER, WEB_SERVER_PASS);
		byte[] contents;

		try {
			// Attempt to recover the target file
			contents = request.DownloadData("ftp://" + BASE_URL + filePath);

			// Apply the files to the current ARMA object being edited
			string data;
			Texture2D tex; 
			switch (fileName) {
			case "model.obj":
				// Create a temporary file
				rawMesh = Encoding.UTF8.GetString(contents);
				fp_object = Application.dataPath + "/Resources/Temp/model.obj";
				System.IO.File.WriteAllText(fp_object, rawMesh);
				// Apply the mesh to the model on screen
				GO_model.GetComponent<MeshFilter>().mesh = (new ObjImporter()).ImportFileRaw(rawMesh);
				break;
			case "transform.txt":
				data = Encoding.UTF8.GetString(contents);
				for (int i = 0; i < 9; i++) {
					transforms[i] = data.Substring(0, data.IndexOf(","));
					data = data.Substring(data.IndexOf(",") + 1);
				}
				GO_model.transform.localPosition    = new Vector3 (float.Parse(transforms[0]), float.Parse(transforms[1]), float.Parse(transforms[2]));
				GO_model.transform.localEulerAngles = new Vector3 (float.Parse(transforms[3]), float.Parse(transforms[4]), float.Parse(transforms[5]));
				GO_model.transform.localScale       = new Vector3 (float.Parse(transforms[6]), float.Parse(transforms[7]), float.Parse(transforms[8]));
				break;
			case "dimension.txt":
				data = Encoding.UTF8.GetString(contents);
				// Model Texture
				{
					dim_tex_x[0] = int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					dim_tex_y[0] = int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					texFormat[0] =           data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
					mipMap   [0] = int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
				}
				// Marker Texture
				{
					dim_tex_x[1] = int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					dim_tex_y[1] = int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					texFormat[1] =           data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
					mipMap   [1] = int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
				}
				// Marker Vuforia ID
				{
					markerID     = data.Substring(0, data.IndexOf(","));            data = data.Substring(data.IndexOf(",") + 1);
				}
				break;
			case "texture.png":
				tex = new Texture2D(dim_tex_x[0], dim_tex_y[0], scr_TexFormatter.Convert(texFormat[0]), mipMap[0] > 1);
				tex.LoadRawTextureData(contents);
				tex.Apply();
				GO_model.GetComponent<MeshRenderer>().material.mainTexture = tex;
				break;
			case "marker.jpg":
				tex = new Texture2D(dim_tex_x[1], dim_tex_y[1], scr_TexFormatter.Convert(texFormat[1]), mipMap[1] > 1);
				tex.LoadRawTextureData(contents);
				tex.Apply();
				GO_marker.GetComponent<MeshRenderer>().material.mainTexture = tex;
				GO_marker.GetComponent<scr_MarkerSizer>().NormalizeScale(tex.width, tex.height);
				break;
			default:
				break;
			}
			DebugMessage ("Downloaded " + messageType + " data.");
		}

		// If the download fails, use default data
		catch (Exception e) {
			Texture2D tex = new Texture2D (2, 2);
			switch (fileName) {
			case "model.obj":
				TextAsset ta = Resources.Load ("defaultmodel") as TextAsset;
				rawMesh = ta.text;
				GO_model.GetComponent<MeshFilter>().mesh = (new ObjImporter()).ImportFileRaw(rawMesh);
				break;
			case "transform.txt":
				transforms = new string[] {
					"0.0", "0.0", "0.0", // Translations
					"0.0", "0.0", "0.0", // Rotations
					"1.0", "1.0", "1.0"  // Scaling
				};
				GO_model.transform.localPosition    = new Vector3 (0.0f, 0.0f, 0.0f);
				GO_model.transform.localEulerAngles = new Vector3 (0.0f, 0.0f, 0.0f);
				GO_model.transform.localScale       = new Vector3 (1.0f, 1.0f, 1.0f);
				GO_marker.transform.localScale      = new Vector3 (1.0f, 1.0f, 1.0f);
				break;
			case "texture.png":
				tex = Resources.Load ("defaultimage") as Texture2D;
				GO_model.GetComponent<MeshRenderer> ().material.mainTexture = tex;
				dim_tex_x [0] = tex.width;
				dim_tex_y [0] = tex.height;
				texFormat [0] = tex.format.ToString();
				mipMap [0] = tex.mipmapCount;
				break;
			case "marker.jpg":
				tex = Resources.Load ("defaultimage") as Texture2D;
				GO_marker.GetComponent<MeshRenderer>().material.mainTexture = tex;
				GO_marker.GetComponent<scr_MarkerSizer>().NormalizeScale(tex.width, tex.height);
				dim_tex_x [1] = tex.width;
				dim_tex_y [1] = tex.height;
				texFormat [1] = tex.format.ToString();
				mipMap[1] = tex.mipmapCount;
				break;
			default:
				break;
			}
			DebugMessage ("Downloaded default " + messageType + " data.");
		}

		yield return null;
		switch (messageType) {
		case "model":
			lock_downloadingData [0] = false;
			break;
		case "transform":
			lock_downloadingData [1] = false;
			break;
		case "dimension":
			lock_downloadingData [2] = false;
			break;
		case "texture":
			lock_downloadingData [3] = false;
			break;
		case "marker":
			lock_downloadingData [4] = false;
			break;
		default:
			break;
		}
	}

	IEnumerator DownloadData() {
		// Items that must be done first
		yield return StartCoroutine(DownloadFile ("dimension", "dimension.txt"));

		// Others
		yield return StartCoroutine (DownloadFile ("model", "model.obj"));
		yield return StartCoroutine (DownloadFile ("transform", "transform.txt"));
		yield return StartCoroutine (DownloadFile ("texture", "texture.png"));
		yield return StartCoroutine (DownloadFile ("marker", "marker.jpg"));
	}

	private void ChangeState(ProgState p) {
		lock_changingStates = true;

		prevState = currState;
		currState = p;

		switch (currState) {

		case ProgState.LOGIN:
			uName = "";
			uPass = "";
			break;

		case ProgState.GALLERY:
			indexCur = 0;
			folderType = "Gallery";
			fName = "New Gallery Name";
			currentGallery = "";
			break;

		case ProgState.PIECE:
			indexCur = 0;
			folderType = "Piece";
			fName = "New Piece Name";
			currentPiece = "";
			transforms = new string[] {
				"0.0", "0.0", "0.0", // Translations
				"0.0", "0.0", "0.0", // Rotations
				"1.0", "1.0", "1.0"  // Scaling
			};
			rawMesh = "";
			dim_tex_x [0] = 0;
			dim_tex_y [0] = 0;
			texFormat [0] = "";
			mipMap [0] = 0;
			dim_tex_x [1] = 0;
			dim_tex_y [1] = 0;
			texFormat [1] = "";
			mipMap [1] = 0;
			GO_camera.GetComponent<scr_CameraControl> ().ResetCamera ();
			break;

		case ProgState.EDIT:
			showEditGUI = true;
			GO_model.GetComponent<MeshRenderer> ().enabled = true;
			GO_marker.GetComponent<MeshRenderer> ().enabled = true;
			mouseState = MouseState.EDITING;
			StartCoroutine(DownloadData ());
			break;

		case ProgState.DELETE:
			uMess = "Delete " + fName + "?";
			break;

		default:
			break;	
		}

		lock_changingStates = false;
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
				fp_object = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
				fp_texture = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
				fp_marker = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
				TextWriter tw = new StreamWriter (Application.persistentDataPath + "/CWBARMA.dat");
				tw.Write (
					fp_object + "\n" +
					fp_texture + "\n" +
					fp_marker + "\n"
				);
				tw.Close ();
			} 
			// Load the last filepaths used for searching
			else {
				TextReader tr = new StreamReader (Application.persistentDataPath + "/CWBARMA.dat");
				fp_object  = tr.ReadLine ();
				fp_texture = tr.ReadLine ();
				fp_marker  = tr.ReadLine ();
				tr.Close ();
			}
		}
		// If it fails, just use defaults and forget about saving the dat file
		catch (Exception e) {
			DebugMessage ("Error: Could not create CWBARMA.dat file!", "error");
			fp_object  = "/";
			fp_texture = "/";
			fp_marker  = "/";
		}
	}

	private void UpdateFilePaths() {
		try {
			TextWriter tw = new StreamWriter (Application.persistentDataPath + "/CWBARMA.dat");
			tw.Write (
				fp_object  + "\n" +
				fp_texture + "\n" +
				fp_marker  + "\n"
			);
			tw.Close ();
		} 
		catch (Exception e) {
			DebugMessage ("Error: Could not create CWBARMA.dat", "error");
		}
	}

	public string GetCurrentTargetName() {
		return "_" + uName + "_" + currentGallery + "_" + currentPiece;
	}

	public void setMarkerID (string id) {
		markerID = id;
	}

	public string getMarkerID () {
		return markerID;
	}
}
