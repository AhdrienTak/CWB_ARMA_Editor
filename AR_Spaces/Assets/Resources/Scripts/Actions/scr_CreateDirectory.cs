using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class scr_CreateDirectory : MonoBehaviour {

	private string BASE_URL;

	void Awake() {
		BASE_URL = this.GetComponent<scr_SceneController>().BASE_URL;
	}

	public IEnumerator Activate() {
		this.GetComponent<scr_SceneController> ().setLock ("cdir", true);

		string uName          = this.GetComponent<scr_SceneController> ().GetUName ();
		string fName          = this.GetComponent<scr_SceneController> ().fName;
		string folderType     = this.GetComponent<scr_SceneController> ().folderType;
		string currentGallery = this.GetComponent<scr_SceneController> ().currentGallery;

		// Check for illegal name
		if (fName.Length < 1) {
			this.GetComponent<scr_SceneController>().DebugMessage("New " + folderType.ToLower() + "'s name must be at least 1 character long.", "error");
			goto end_CreateFile_B;
		}
		string[] INVALID_CHAR = this.GetComponent<scr_SceneController> ().INVALID_CHAR;
		for (int i = 0; i < INVALID_CHAR.Length; i++) {
			if (fName.Contains (INVALID_CHAR [i])) {
				this.GetComponent<scr_SceneController>().DebugMessage("New " + folderType.ToLower() + "'s name contains invalid characters.", "error");
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
			this.GetComponent<scr_SceneController>().DebugMessage(www.error, "error");
			goto end_CreateFile_A;
		}

		// Handle File Creation
		this.GetComponent<scr_SceneController>().DebugMessage("Directory created.");

		end_CreateFile_A:
		www.Dispose ();
		yield return StartCoroutine (this.GetComponent<scr_RefreshListings>().Activate());
		end_CreateFile_B:
		this.GetComponent<scr_SceneController> ().setLock ("cdir", false);
	}
}
