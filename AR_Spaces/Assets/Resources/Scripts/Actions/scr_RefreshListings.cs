using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class scr_RefreshListings : MonoBehaviour {

	private string BASE_URL;

	void Awake() {
		BASE_URL = this.GetComponent<scr_SceneController>().BASE_URL;
	}

	public IEnumerator Activate() {
		this.GetComponent<scr_SceneController> ().setLock ("list", true);

		string uName          = this.GetComponent<scr_SceneController> ().GetUName ();
		string folderType     = this.GetComponent<scr_SceneController> ().folderType;
		string currentGallery = this.GetComponent<scr_SceneController> ().currentGallery;

		WWWForm form = new WWWForm();
		form.AddField("author", uName);
		form.AddField("type", folderType);
		form.AddField("gallery", currentGallery);
		UnityWebRequest www = UnityWebRequest.Post("http://" + BASE_URL + "Galleries/listDirectories.php", form);
		www.chunkedTransfer = false;
		yield return www.SendWebRequest ();

		// Handle Network Errors
		if(www.isNetworkError || www.isHttpError) {
			this.GetComponent<scr_SceneController> ().DebugMessage(www.error, "error");
			goto end_RefreshListings_A;
		}

		// Retrieve folders
		this.GetComponent<scr_SceneController> ().DebugMessage("Searching for " + folderType.ToLower() + " directories.");

		// Parse the downloaded contents
		string foldersList = www.downloadHandler.text;

		if (foldersList.Equals ("failed")) {
			this.GetComponent<scr_SceneController> ().DebugMessage("Failed searching for " + folderType.ToLower() + " directories.", "error");
			goto end_RefreshListings_A;
		}

		this.GetComponent<scr_SceneController> ().listings = new List<string>();
		while (foldersList.Length > 1) {
			int nextComma = foldersList.IndexOf (",");
			this.GetComponent<scr_SceneController> ().listings.Add (foldersList.Substring(0, nextComma));
			foldersList = foldersList.Substring (nextComma + 1);
		}

		this.GetComponent<scr_SceneController> ().DebugMessage ("Finished listing directories.");

		end_RefreshListings_A:
		www.Dispose ();
		end_RefreshListings_B:
		this.GetComponent<scr_SceneController> ().setLock ("list", false);
	}
}
