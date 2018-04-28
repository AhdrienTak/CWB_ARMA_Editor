using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;
using System;

public class scr_Download : MonoBehaviour {

	private string BASE_URL;
	private GameObject MS_GUI;
	private GameObject GO_light;
	private GameObject GO_model;
	private GameObject GO_marker;
	private TextAsset  defaultModel;


	void Awake() {
		BASE_URL     = this.GetComponent<scr_SceneController> ().BASE_URL;
		MS_GUI       = GameObject.Find ("MASTER_GUI");
		GO_light     = this.GetComponent<scr_SceneController> ().GetGOLight();
		GO_model     = this.GetComponent<scr_SceneController> ().GetGOModel();
		GO_marker    = this.GetComponent<scr_SceneController> ().GetGOMarker();
		defaultModel = this.GetComponent<scr_SceneController> ().GetDefaultModel();
	}

	public IEnumerator Activate() {
		// Items that must be done first
		yield return StartCoroutine(DownloadFile ("metadata", "metadata.txt"));

		// Others
		yield return StartCoroutine (DownloadFile ("model", "model.obj"));
		yield return StartCoroutine (DownloadFile ("transform", "transform.txt"));
		yield return StartCoroutine (DownloadFile ("texture", "texture.png"));
		yield return StartCoroutine (DownloadFile ("normal", "normal.jpg"));
		yield return StartCoroutine (DownloadFile ("marker", "marker.jpg"));

		// Vuforia Marker ID
		yield return StartCoroutine (DownloadFile ("markerID", "markerID.txt"));

		// Enable Editing Mode
		MS_GUI.GetComponent<scr_Edit>().PassVars(Screen.width / 2.0f, Screen.height / 2.0f, this.GetComponent<scr_SceneController> ().GetUName());
		MS_GUI.GetComponent<scr_Edit> ().ShowGUI (true);
	}

	public IEnumerator DownloadFile(string messageType, string fileName) {
		switch (messageType) {
		case "model":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 0, true);
			break;
		case "transform":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 1, true);
			break;
		case "metadata":
		case "markerID":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 2, true);
			break;
		case "texture":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 3, true);
			break;
		case "normal":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 4, true);
			break;
		case "marker":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 5, true);
			break;
		default:
			break;
		}

		this.GetComponent<scr_SceneController> ().DebugMessage("Downloading " + messageType + " data...");
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
		WebClient request = new WebClient ();
		request.Credentials = new NetworkCredential (this.GetComponent<scr_SceneController> ().GetWSU(), this.GetComponent<scr_SceneController> ().GetWSP());
		byte[] contents;

		try {
			// Attempt to recover the target file
			contents = request.DownloadData("ftp://" + this.GetComponent<scr_SceneController> ().BASE_URL + filePath);

			// Apply the files to the current ARMA object being edited
			string data;
			Texture2D tex; 
			switch (fileName) {
			case "model.obj":
				// Create a temporary file
				this.GetComponent<scr_SceneController> ().rawMesh = Encoding.UTF8.GetString(contents);
				this.GetComponent<scr_SceneController> ().fp_object = Application.persistentDataPath + "/Temp/model.obj";
				System.IO.File.WriteAllText(this.GetComponent<scr_SceneController> ().fp_object, this.GetComponent<scr_SceneController> ().rawMesh);
				// Apply the mesh to the model on screen
				GO_model.GetComponent<MeshFilter>().mesh = (new ObjImporter()).ImportFileRaw(this.GetComponent<scr_SceneController> ().rawMesh);
				break;
			case "transform.txt":
				data = Encoding.UTF8.GetString(contents);
				for (int i = 0; i < 9; i++) {
					this.GetComponent<scr_SceneController> ().txform_mod[i] = data.Substring(0, data.IndexOf(","));
					data = data.Substring(data.IndexOf(",") + 1);
				}
				for (int i = 3; i < 5; i++) {
					this.GetComponent<scr_SceneController> ().txform_lit[i] = data.Substring(0, data.IndexOf(","));
					data = data.Substring(data.IndexOf(",") + 1);
				}
				GO_model.transform.localPosition    = new Vector3 (
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[0]),
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[1]),
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[2])
				);
				GO_model.transform.localEulerAngles = new Vector3 (
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[3]),
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[4]),
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[5])
				);
				GO_model.transform.localScale       = new Vector3 (
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[6]),
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[7]),
					float.Parse(this.GetComponent<scr_SceneController> ().txform_mod[8])
				);
				GO_light.transform.localEulerAngles = new Vector3 (
					float.Parse(this.GetComponent<scr_SceneController> ().txform_lit[3]),
					float.Parse(this.GetComponent<scr_SceneController> ().txform_lit[4]),
					0.0f
				);
				break;
			case "metadata.txt":
				data = Encoding.UTF8.GetString(contents);
				// Model Texture
				{
					this.GetComponent<scr_SceneController> ().dim_tex_x[0] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().dim_tex_y[0] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().texFormat[0] =             data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().mipMap   [0] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
				}
				// Normal Map
				{
					this.GetComponent<scr_SceneController> ().dim_tex_x[1] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().dim_tex_y[1] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().texFormat[1] =             data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().mipMap   [1] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
				}
				// Marker Texture
				{
					this.GetComponent<scr_SceneController> ().dim_tex_x[2] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().dim_tex_y[2] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().texFormat[2] =             data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().mipMap   [2] =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
				}
				// Specularity Level
				{
					this.GetComponent<scr_SceneController> ().specularity  =             data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
				}
				// Light Color
				{
					this.GetComponent<scr_SceneController> ().color_lite[0]=             data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().color_lite[1]=             data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
					this.GetComponent<scr_SceneController> ().color_lite[2]=             data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
				}
				// Light Intensity
				{
					this.GetComponent<scr_SceneController> ().intensity    =             data.Substring(0, data.IndexOf(",")) ; data = data.Substring(data.IndexOf(",") + 1);
				}
				// Shader Number
				{
					int matNum   =   int.Parse(data.Substring(0, data.IndexOf(","))); data = data.Substring(data.IndexOf(",") + 1);
					{
						this.GetComponent<scr_SceneController> ().shader_bump  = false;
						this.GetComponent<scr_SceneController> ().shader_spec  = false;
						this.GetComponent<scr_SceneController> ().shader_tran  = false;
						if (matNum % 4  > 1) this.GetComponent<scr_SceneController> ().shader_spec  = true;
						if (matNum % 2 == 1) this.GetComponent<scr_SceneController> ().shader_bump  = true;
						if (matNum      > 3) this.GetComponent<scr_SceneController> ().shader_tran  = true;
					}
					this.GetComponent<scr_SceneController> ().SetModelShader ();
				}
				break;
			case "markerID.txt":
				data = Encoding.UTF8.GetString(contents);
				this.GetComponent<scr_SceneController> ().SetMarkerID(data);
				break;
			case "texture.png":
				tex = new Texture2D(
					this.GetComponent<scr_SceneController> ().dim_tex_x[0],
					this.GetComponent<scr_SceneController> ().dim_tex_y[0],
					scr_TexFormatter.Convert(this.GetComponent<scr_SceneController> ().texFormat[0]),
					this.GetComponent<scr_SceneController> ().mipMap[0] > 1
				);
				tex.LoadRawTextureData(contents);
				tex.Apply();
				GO_model.GetComponent<MeshRenderer>().material.mainTexture = tex;
				break;
			case "normal.jpg":
				tex = new Texture2D(
					this.GetComponent<scr_SceneController> ().dim_tex_x[1],
					this.GetComponent<scr_SceneController> ().dim_tex_y[1],
					scr_TexFormatter.Convert(this.GetComponent<scr_SceneController> ().texFormat[1]),
					this.GetComponent<scr_SceneController> ().mipMap[1] > 1
				);
				tex.LoadRawTextureData(contents);
				tex.Apply();
				GO_marker.GetComponent<MeshRenderer>().material.SetTexture("_BumpMap", tex);
				break;
			case "marker.jpg":
				tex = new Texture2D(
					this.GetComponent<scr_SceneController> ().dim_tex_x[2],
					this.GetComponent<scr_SceneController> ().dim_tex_y[2],
					scr_TexFormatter.Convert(this.GetComponent<scr_SceneController> ().texFormat[2]),
					this.GetComponent<scr_SceneController> ().mipMap[2] > 1
				);
				tex.LoadRawTextureData(contents);
				tex.Apply();
				GO_marker.GetComponent<MeshRenderer>().material.mainTexture = tex;
				GO_marker.GetComponent<scr_MarkerSizer>().NormalizeScale(tex.width, tex.height);
				break;
			default:
				break;
			}
			this.GetComponent<scr_SceneController> ().DebugMessage ("Downloaded " + messageType + " data.");
		}

		// If the download fails, use default data
		catch (Exception e) {
			Texture2D tex = new Texture2D (2, 2);
			switch (fileName) {
			case "model.obj":
				this.GetComponent<scr_SceneController> ().rawMesh = defaultModel.text;
				GO_model.GetComponent<MeshFilter> ().mesh = (new ObjImporter ()).ImportFileRaw (this.GetComponent<scr_SceneController> ().rawMesh);
				break;
			case "transform.txt":
				this.GetComponent<scr_SceneController> ().txform_mod = new string[] {
					"0.0", "0.0", "0.0", // Translations
					"0.0", "0.0", "0.0", // Rotations
					"1.0", "1.0", "1.0"  // Scaling
				};
				this.GetComponent<scr_SceneController> ().txform_lit = new string[] {
					 "0.0", "0.0", "0.0", // Translations
					"20.0", "0.0", "0.0", // Rotations
					 "1.0", "1.0", "1.0"  // Scaling
				};
				GO_model.transform.localPosition    = new Vector3 ( 0.0f, 0.0f, 0.0f);
				GO_model.transform.localEulerAngles = new Vector3 ( 0.0f, 0.0f, 0.0f);
				GO_model.transform.localScale       = new Vector3 ( 1.0f, 1.0f, 1.0f);
				GO_light.transform.localEulerAngles = new Vector3 (20.0f, 0.0f, 0.0f);
				GO_marker.transform.localScale      = new Vector3 ( 1.0f, 1.0f, 1.0f);
				break;
			case "markerID.txt":
				this.GetComponent<scr_SceneController> ().SetMarkerID ("");
				break;
			case "texture.png":
				tex = Resources.Load ("defaultimage") as Texture2D;
				GO_model.GetComponent<MeshRenderer> ().material.mainTexture = tex;
				this.GetComponent<scr_SceneController> ().dim_tex_x [0] = tex.width;
				this.GetComponent<scr_SceneController> ().dim_tex_y [0] = tex.height;
				this.GetComponent<scr_SceneController> ().texFormat [0] = tex.format.ToString();
				this.GetComponent<scr_SceneController> ().mipMap [0] = tex.mipmapCount;
				break;
			case "normal.jpg":
				tex = Resources.Load ("defaultimage") as Texture2D;
				GO_marker.GetComponent<MeshRenderer> ().material.SetTexture ("_BumpMap", tex);
				this.GetComponent<scr_SceneController> ().dim_tex_x [1] = tex.width;
				this.GetComponent<scr_SceneController> ().dim_tex_y [1] = tex.height;
				this.GetComponent<scr_SceneController> ().texFormat [1] = tex.format.ToString();
				this.GetComponent<scr_SceneController> ().mipMap[1] = tex.mipmapCount;
				break;
			case "marker.jpg":
				tex = Resources.Load ("defaultimage") as Texture2D;
				GO_marker.GetComponent<MeshRenderer>().material.mainTexture = tex;
				GO_marker.GetComponent<scr_MarkerSizer>().NormalizeScale(tex.width, tex.height);
				this.GetComponent<scr_SceneController> ().dim_tex_x [2] = tex.width;
				this.GetComponent<scr_SceneController> ().dim_tex_y [2] = tex.height;
				this.GetComponent<scr_SceneController> ().texFormat [2] = tex.format.ToString();
				this.GetComponent<scr_SceneController> ().mipMap[2] = tex.mipmapCount;
				break;
			default:
				break;
			}
			this.GetComponent<scr_SceneController> ().DebugMessage ("Downloaded default " + messageType + " data.");
		}

		yield return null;
		switch (messageType) {
		case "model":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 0, false);
			break;
		case "transform":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 1, false);
			break;
		case "metadata":
		case "markerID":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 2, false);
			break;
		case "texture":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 3, false);
			break;
		case "normal":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 4, false);
			break;
		case "marker":
			this.GetComponent<scr_SceneController> ().setLock ("dnld", 5, false);
			break;
		default:
			break;
		}
	}

}
