using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Light : MonoBehaviour {

	private readonly float INTENSITY = 0.5f;

	private float intensity;


	void Awake() {
		setDefaultVariables ();
	}

	public void setDefaultVariables() {
		Light light = this.GetComponent<Light> ();

		this.transform.eulerAngles = new Vector3 (20f, 0f, 0f);

		intensity = INTENSITY;

		light.intensity = intensity;
	}

	public void setRotation(string[] rot) {
		this.transform.eulerAngles = new Vector3(float.Parse(rot[0]), float.Parse(rot[1]), float.Parse(rot[2]));
	}

	public void setIntensity(float amount) {
		if (amount < 0.0f)
			amount = 0.0f;
		else if (amount > 1.0f)
			amount = 1.0f;

		intensity = amount;
		this.GetComponent<Light> ().intensity = intensity;
	}

}
