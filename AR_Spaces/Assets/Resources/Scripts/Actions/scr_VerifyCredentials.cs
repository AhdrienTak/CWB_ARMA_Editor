using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class scr_VerifyCredentials : MonoBehaviour {

	private string BASE_URL;

	void Awake() {
		BASE_URL = this.GetComponent<scr_SceneController>().BASE_URL;
	}

	public IEnumerator Activate(string uName, string uPass) {
		this.GetComponent<scr_SceneController> ().setLock ("auth", true);

		// Handle empty inputs
		if (uName.Length < 1 || uPass.Length < 1) {
			this.GetComponent<scr_SceneController> ().DebugMessage ("Both username and password must be at least 1 character long.", "error");
			goto end_VerifyCredentials_B;
		}

		// Handle Invalid User Name
		string[] INVALID_CHAR = this.GetComponent<scr_SceneController> ().INVALID_CHAR;
		for (int i = 0; i < INVALID_CHAR.Length; i++) {
			if (uName.Contains (INVALID_CHAR [i])) {
				this.GetComponent<scr_SceneController> ().DebugMessage ("Username contains illegal characters.", "error");
				goto end_VerifyCredentials_B;
			}
		}

		// Handle Invalid Password
		string[] INVALID_PASS = this.GetComponent<scr_SceneController> ().INVALID_PASS;
		for (int i = 0; i < INVALID_PASS.Length; i++) {
			if (uPass.Contains (INVALID_PASS [i])) {
				this.GetComponent<scr_SceneController> ().DebugMessage ("Password contains illegal characters.", "error");
				goto end_VerifyCredentials_B;
			}
		}

		// Pull the login credentials data off of the web server
		UnityWebRequest www = UnityWebRequest.Get("http://" + BASE_URL + "Credentials/_" + uName.ToLower() + ".txt");
		www.chunkedTransfer = false;
		yield return www.SendWebRequest ();

		// Handle Network Error or File Error
		if(www.isNetworkError || www.isHttpError) {
			this.GetComponent<scr_SceneController> ().DebugMessage ("Unable to verify user: " + uName, "error");
			goto end_VerifyCredentials_A;
		}

		// Check Password
		if (www.downloadHandler.text.Equals (uPass)) {
			this.GetComponent<scr_SceneController> ().ChangeState (scr_SceneController.ProgState.GALLERY);
			this.GetComponent<scr_SceneController> ().DebugMessage ("Login Success!");
			this.GetComponent<scr_SceneController> ().SetUName (uName.ToLower ());
			yield return StartCoroutine(this.GetComponent<scr_RefreshListings>().Activate());
		}
		else {
			this.GetComponent<scr_SceneController> ().DebugMessage ("Unable to verify user: " + uName, "error");
		} 

		end_VerifyCredentials_A:
		www.Dispose ();
		end_VerifyCredentials_B:
		this.GetComponent<scr_SceneController> ().setLock ("auth", false);
	}
}
