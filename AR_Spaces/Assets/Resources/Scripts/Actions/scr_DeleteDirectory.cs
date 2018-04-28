using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class scr_DeleteDirectory : MonoBehaviour {

	private string BASE_URL;

	void Awake() {
		BASE_URL = this.GetComponent<scr_SceneController>().BASE_URL;
	}

	public IEnumerator Activate(string folderName) {
		this.GetComponent<scr_SceneController> ().setLock ("ddir", true);
		this.GetComponent<scr_SceneController> ().fName = folderName;
		this.GetComponent<scr_SceneController> ().ChangeState (scr_SceneController.ProgState.DELETE);
		yield return null;
		this.GetComponent<scr_SceneController> ().setLock ("ddir", false);
	}

	public IEnumerator Activate(bool affirmation) {
		this.GetComponent<scr_SceneController> ().setLock ("ddir", true);

		if (affirmation) {

			string type = "";
			if (this.GetComponent<scr_SceneController> ().GetPreviousProgState() == scr_SceneController.ProgState.GALLERY)
				type = "Gallery";
			else if (this.GetComponent<scr_SceneController> ().GetPreviousProgState()  == scr_SceneController.ProgState.PIECE)
				type = "Piece";

			string fName = this.GetComponent<scr_SceneController> ().fName;
			string uName = this.GetComponent<scr_SceneController> ().GetUName ();
			string currentGallery = this.GetComponent<scr_SceneController> ().currentGallery;

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
				this.GetComponent<scr_SceneController> ().DebugMessage (www.error, "error");
			} 

			// Wait for deletion
			else {
				this.GetComponent<scr_SceneController> ().DebugMessage ("Merging Spaces files deleted.");
			}

			www.Dispose ();
			yield return StartCoroutine (this.GetComponent<scr_RefreshListings>().Activate());

		}
		else {
			this.GetComponent<scr_SceneController> ().DebugMessage ("Action canceled.");
		}

		this.GetComponent<scr_SceneController> ().ChangeState (this.GetComponent<scr_SceneController> ().GetPreviousProgState());
		this.GetComponent<scr_SceneController> ().setLock ("ddir", false);
	}
}
