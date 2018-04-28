using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System;
using System.Text;

public class scr_Save : MonoBehaviour {
	
	private string BASE_URL;
	private GameObject GO_model;
	private GameObject GO_marker;

	void Awake() {
		BASE_URL  = this.GetComponent<scr_SceneController>().BASE_URL;
		GO_model  = this.GetComponent<scr_SceneController> ().GetGOModel();
		GO_marker = this.GetComponent<scr_SceneController> ().GetGOMarker();
	}

	public IEnumerator Activate() {
		this.GetComponent<scr_SceneController> ().setLock ("save", true);

		// Save model 
		StartCoroutine(UploadFile("model", Encoding.UTF8.GetBytes (this.GetComponent<scr_SceneController> ().rawMesh), "model.obj"));

		// Save model transform in transform.txt
		string transformData = 
			this.GetComponent<scr_SceneController> ().txform_mod [0] + "," +  // ~ X Translation (3D model)
			this.GetComponent<scr_SceneController> ().txform_mod [1] + "," +  // ~ Y Translation (3D model)
			this.GetComponent<scr_SceneController> ().txform_mod [2] + "," +  // ~ Z Translation (3D model)
			this.GetComponent<scr_SceneController> ().txform_mod [3] + "," +  // ~ X Rotation    (3D model)
			this.GetComponent<scr_SceneController> ().txform_mod [4] + "," +  // ~ Y Rotation    (3D model)
			this.GetComponent<scr_SceneController> ().txform_mod [5] + "," +  // ~ Z Rotation    (3D model)
			this.GetComponent<scr_SceneController> ().txform_mod [6] + "," +  // ~ X Scale       (3D model)
			this.GetComponent<scr_SceneController> ().txform_mod [7] + "," +  // ~ Y Scale       (3D model)
			this.GetComponent<scr_SceneController> ().txform_mod [8] + "," +  // ~ Z Scale       (3D model)
			this.GetComponent<scr_SceneController> ().txform_lit [3] + "," +  // ~ X Rotation    (directional light)
			this.GetComponent<scr_SceneController> ().txform_lit [4] + "," ;  // ~ Y Rotation    (directional light)
		
		StartCoroutine (UploadFile ("transform", Encoding.UTF8.GetBytes (transformData), "transform.txt"));

		// Get the shader number
		int shadNum = GO_model.GetComponent<scr_MaterialSwap>().getMaterialNumber();

		// Save image file dimensions in metadata.txt
		string imageMetaData =
			this.GetComponent<scr_SceneController> ().dim_tex_x  [0] + "," + // (int)           Width          of texture
			this.GetComponent<scr_SceneController> ().dim_tex_y  [0] + "," + // (int)           Height         of texture 
			this.GetComponent<scr_SceneController> ().texFormat  [0] + "," + // (TextureFormat) Format         of texture
			this.GetComponent<scr_SceneController> ().mipMap     [0] + "," + // (int)           MipMap count   of texture
			this.GetComponent<scr_SceneController> ().dim_tex_x  [1] + "," + // (int)           Width          of normal map
			this.GetComponent<scr_SceneController> ().dim_tex_y  [1] + "," + // (int)           Height         of normal map
			this.GetComponent<scr_SceneController> ().texFormat  [1] + "," + // (TextureFormat) Format         of normal map
			this.GetComponent<scr_SceneController> ().mipMap     [1] + "," + // (int)           MipMap count   of normal map
			this.GetComponent<scr_SceneController> ().dim_tex_x  [2] + "," + // (int)           Width          of marker
			this.GetComponent<scr_SceneController> ().dim_tex_y  [2] + "," + // (int)           Height         of marker
			this.GetComponent<scr_SceneController> ().texFormat  [2] + "," + // (TextureFormat) Format         of marker
			this.GetComponent<scr_SceneController> ().mipMap     [2] + "," + // (int)           MipMap count   of marker
			this.GetComponent<scr_SceneController> ().specularity    + "," + // (float)         Specularity    of model
			this.GetComponent<scr_SceneController> ().color_lite [0] + "," + // (int) (0-255)   Direcitonal Light Red   
			this.GetComponent<scr_SceneController> ().color_lite [1] + "," + // (int) (0-255)   Direcitonal Light Green
			this.GetComponent<scr_SceneController> ().color_lite [2] + "," + // (int) (0-255)   Direcitonal Light Blue
			this.GetComponent<scr_SceneController> ().intensity      + "," + // (float)         Direcitonal Light Intensity
			                                          shadNum        + "," ; // (int)           Shader number of model
		
		StartCoroutine(UploadFile("metadata", Encoding.UTF8.GetBytes (imageMetaData), "metadata.txt"));

		// Save texture
		StartCoroutine(UploadFile("texture", ((Texture2D) GO_model.GetComponent<MeshRenderer> ().material.mainTexture).GetRawTextureData(), "texture.png"));

		// Save normal
		try {
			StartCoroutine(UploadFile("normal", ((Texture2D) GO_model.GetComponent<MeshRenderer> ().material.GetTexture("_BumpMap")).GetRawTextureData(), "normal.jpg"));
		}
		catch (Exception e) {
			this.GetComponent<scr_SceneController> ().DebugMessage ("No normal map detected...");
		}

		// Save marker
		StartCoroutine(UploadFile("marker", ((Texture2D) GO_marker.GetComponent<MeshRenderer> ().material.mainTexture).GetRawTextureData(), "marker.jpg"));

		yield return null;
		this.GetComponent<scr_SceneController> ().setLock ("save", false);
	}

	public IEnumerator UploadFile(string messageType, byte[] contents, string fileName) {
		string contentType = "";
		bool convertImage = false;
		string imageMime = "";

		switch (messageType) {
		case "model":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 0, true);
			break;
		case "transform":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 1, true);
			break;
		case "metadata":
		case "markerID":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 2, true);
			break;
		case "texture":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 3, true);
			convertImage = true;
			imageMime = "png";
			break;
		case "normal":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 4, true);
			convertImage = true;
			imageMime = "jpg";
			break;
		case "marker":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 5, true);
			convertImage = true;
			imageMime = "jpg";
			break;
		default:
			break;
		}

		this.GetComponent<scr_SceneController> ().DebugMessage ("Uploading " + messageType + " data...");
		string filePath = 
			"Galleries/_" +
			this.GetComponent<scr_SceneController> ().GetUName() +
			"/_" +
			this.GetComponent<scr_SceneController> ().currentGallery +
			this.GetComponent<scr_SceneController> ().liveGallery + 
			"/_" +
			this.GetComponent<scr_SceneController> ().currentPiece +
			this.GetComponent<scr_SceneController> ().livePiece +
			"/" +
			fileName;
		FtpWebRequest wrq;
		FtpWebResponse wrp;
		Stream requestStream;

		try {
			wrq = (FtpWebRequest)WebRequest.Create ("ftp://" + this.GetComponent<scr_SceneController> ().BASE_URL + filePath);
			wrq.Method = WebRequestMethods.Ftp.UploadFile;
			wrq.Credentials = new NetworkCredential (this.GetComponent<scr_SceneController> ().GetWSU(), this.GetComponent<scr_SceneController> ().GetWSP());
			wrq.ContentLength = contents.Length;
			wrq.UseBinary = true;
			requestStream = wrq.GetRequestStream ();
			requestStream.Write (contents, 0, contents.Length);
			requestStream.Close ();
			wrp = (FtpWebResponse)wrq.GetResponse ();
			this.GetComponent<scr_SceneController> ().DebugMessage (wrp.StatusDescription);
			wrp.Close ();
			this.GetComponent<scr_SceneController> ().DebugMessage ("Uploading " + messageType + " data complete.");
		} catch (Exception e) {
			this.GetComponent<scr_SceneController> ().DebugMessage ("Uploading " + messageType + " data failed.", "error");
		}

		yield return null;
		switch (messageType) {
		case "model":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 0, false);
			break;
		case "transform":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 1, false);
			break;
		case "metadata":
		case "markerID":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 2, false);
			break;
		case "texture":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 3, false);
			break;
		case "normal":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 4, false);
			break;
		case "marker":
			this.GetComponent<scr_SceneController> ().setLock ("upld", 5, false);
			break;
		default:
			break;
		}
	}

	public IEnumerator UploadVuforiaMarker() {
		this.GetComponent<scr_SceneController> ().setLock ("save", true);

		string uName = this.GetComponent<scr_SceneController> ().GetUName ();
		string currentGallery = this.GetComponent<scr_SceneController> ().currentGallery;
		string currentPiece = this.GetComponent<scr_SceneController> ().currentPiece;

		// Save image file dimensions
		string basePath         = "Galleries/_" + uName + "/_" + currentGallery + "o/_" + currentPiece + "o/";
		string filePathTxforms  = basePath + "transform.txt"; // (real .txt) readable text
		string filePathMetaData = basePath + "metadata.txt";  // (real .txt) readable text
		string filePathModel    = basePath + "model.obj";     // (real .obj) readable text
		string filePathDiff     = basePath + "texture.png";   // (fake .png) actually a byte array
		string filePathNorm     = basePath + "normal.jpg";    // (fake .jpg) actually a byte array 

		string imageMetaData =
			filePathTxforms  + "," + // (string) Web Server Filepath of model transform offsets
			filePathMetaData + "," + // (string) Web Server Filepath of texture metadata items
			filePathModel    + "," + // (string) Web Server Filepath of model's mesh
			filePathDiff     + "," + // (string) Web Server Filepath of texture (diffuse)
			filePathNorm     + "," ; // (string) Web Server Filepath of normal map

		// Upload image marker to Vuforia Cloud
		this.GetComponent<scr_SceneController> ().DebugMessage("Uploading image target to Vuforia cloud...");
		Texture2D imageTarget = (Texture2D) GO_marker.GetComponent<MeshRenderer> ().material.mainTexture;
		string markerName = this.GetComponent<scr_SceneController> ().GetCurrentTargetName() + "_marker";
		yield return StartCoroutine (this.GetComponent<CloudUploading>().CallPostTarget (imageTarget, markerName, imageMetaData));

		// Upload the image marker's ID to the web server
		yield return StartCoroutine(UploadFile("markerID", Encoding.UTF8.GetBytes (this.GetComponent<scr_SceneController> ().markerID), "markerID.txt"));

		this.GetComponent<scr_SceneController> ().setLock ("save", false);
	}

}
