using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class scr_CreateCredentials : MonoBehaviour {

	private string BASE_URL;

	void Awake() {
		BASE_URL = this.GetComponent<scr_SceneController>().BASE_URL;
	}

	public IEnumerator Activate(string uName, string uPass) {
		this.GetComponent<scr_SceneController> ().setLock ("auth", true);

		// Handle empty inputs
		if (uName.Length < 1 || uPass.Length < 1) {
			this.GetComponent<scr_SceneController> ().DebugMessage ("Both username and password must be at least 1 character long.", "error");
			goto end_CreateCredentials_B;
		}

		// Handle Invalid User Name
		string[] INVALID_CHAR = this.GetComponent<scr_SceneController> ().INVALID_CHAR;
		for (int i = 0; i < INVALID_CHAR.Length; i++) {
			if (uName.Contains (INVALID_CHAR [i])) {
				this.GetComponent<scr_SceneController> ().DebugMessage ("Username contains illegal characters.", "error");
				goto end_CreateCredentials_B;
			}
		}

		// Handle Invalid Password
		string[] INVALID_PASS = this.GetComponent<scr_SceneController> ().INVALID_PASS;
		for (int i = 0; i < INVALID_PASS.Length; i++) {
			if (uPass.Contains (INVALID_PASS [i])) {
				this.GetComponent<scr_SceneController> ().DebugMessage ("Password contains illegal characters.", "error");
				goto end_CreateCredentials_B;
			}
		}

		// Check to make sure the username does not already exist
		WWWForm form = new WWWForm();
		form.AddField("user", uName.ToLower());
		form.AddField("pass", uPass          );
		UnityWebRequest www = UnityWebRequest.Post("http://" + BASE_URL + "Credentials/credentials.php", form);
		www.chunkedTransfer = false;
		yield return www.SendWebRequest ();

		// Handle Network Error or File Error
		if (www.isNetworkError || www.isHttpError) {
			this.GetComponent<scr_SceneController> ().DebugMessage (www.error, "error");
			goto end_CreateCredentials_A;
		}

		else {
			this.GetComponent<scr_SceneController> ().DebugMessage (www.downloadHandler.text);
		}

		end_CreateCredentials_A:
		www.Dispose ();
		end_CreateCredentials_B:
		this.GetComponent<scr_SceneController> ().setLock ("auth", false);
	}
}
