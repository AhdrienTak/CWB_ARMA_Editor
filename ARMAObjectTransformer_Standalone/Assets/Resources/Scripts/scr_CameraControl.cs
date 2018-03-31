using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_CameraControl : MonoBehaviour {

	private readonly Vector3 DEF_POS = new Vector3 ( 00.00f,  02.25f, -04.00f);
	private readonly Vector3 DEF_ROT = new Vector3 ( 20.00f,  00.00f,  00.00f);

	private readonly float SPD_TRA = 10.0f;
	private readonly float SPD_ROT = 50.0f;


	void Awake () {
		ResetCamera ();
	}

	public void ResetCamera() {
		this.transform.localPosition = DEF_POS;
		this.transform.localEulerAngles = DEF_ROT;
	}

	public void Translate(string direction) {
		float amount = SPD_TRA * Time.deltaTime;
		switch (direction) {
		case "D": // Down
			transform.position += this.transform.up * -amount;
			break;
		case "U": // Up
			transform.position += this.transform.up *  amount;
			break;
		case "L": // Left
			transform.position += this.transform.right * -amount;
			break;
		case "R": // Right
			transform.position += this.transform.right *  amount;
			break;
		case "O": // Out
			transform.position += this.transform.forward * -amount;
			break;
		case "I": // In
			transform.position += this.transform.forward *  amount;
			break;
		default:
			break;
		}
	}

	public void Rotate(float x, float y) {
		Vector3 rotateValue = new Vector3 (x * SPD_ROT * Time.deltaTime, -y * SPD_ROT * Time.deltaTime, 0);
		this.transform.eulerAngles = this.transform.eulerAngles - rotateValue;
	}

}
