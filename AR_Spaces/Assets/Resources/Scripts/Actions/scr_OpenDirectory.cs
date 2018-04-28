using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_OpenDirectory : MonoBehaviour {

	public IEnumerator Activate(string folderName) {
		this.GetComponent<scr_SceneController> ().setLock ("odir", true);
		if (this.GetComponent<scr_SceneController> ().GetCurrentProgState() == scr_SceneController.ProgState.GALLERY) {
			this.GetComponent<scr_SceneController> ().currentGallery = folderName;
			this.GetComponent<scr_SceneController> ().ChangeState (scr_SceneController.ProgState.PIECE);
		}
		else if (this.GetComponent<scr_SceneController> ().GetCurrentProgState() == scr_SceneController.ProgState.PIECE) {
			this.GetComponent<scr_SceneController> ().currentPiece = folderName;
			this.GetComponent<scr_SceneController> ().ChangeState (scr_SceneController.ProgState.EDIT);
		}
		yield return StartCoroutine (this.GetComponent<scr_RefreshListings>().Activate());
		this.GetComponent<scr_SceneController> ().setLock ("odir", false);
	}
}
