using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class scr_LockDirectory : MonoBehaviour {

	private string BASE_URL;

	void Awake() {
		BASE_URL = this.GetComponent<scr_SceneController>().BASE_URL;
	}

	public IEnumerator Activate(string name, string lockState) {
		this.GetComponent<scr_SceneController> ().setLock ("ldir", true);

		string uName = this.GetComponent<scr_SceneController> ().GetUName();
		string currentGallery = this.GetComponent<scr_SceneController> ().currentGallery;

		WWWForm form = new WWWForm ();
		form.AddField ("author", uName);
		if (this.GetComponent<scr_SceneController> ().GetCurrentProgState() == scr_SceneController.ProgState.GALLERY) {
			form.AddField ("type",    "Gallery");
			form.AddField ("gallery", name);
		} else {
			form.AddField ("type",    "Piece");
			form.AddField ("gallery", currentGallery);
			form.AddField ("piece",   name);
		}
		form.AddField ("lock", lockState);

		UnityWebRequest www = UnityWebRequest.Post ("http://" + BASE_URL + "Galleries/lockDirectory.php", form);
		www.chunkedTransfer = false;
		yield return www.SendWebRequest();

		// Handle Network Errors
		if (www.isNetworkError || www.isHttpError) {
			this.GetComponent<scr_SceneController> ().DebugMessage (www.error, "error");
		} 

		// Wait for deletion
		else {
			this.GetComponent<scr_SceneController> ().DebugMessage(www.downloadHandler.text);
		}

		www.Dispose ();
		yield return StartCoroutine (this.GetComponent<scr_RefreshListings>().Activate());

		this.GetComponent<scr_SceneController> ().setLock ("ldir", false);
	}

}
