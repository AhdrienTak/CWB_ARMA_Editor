using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_MaterialSwap : MonoBehaviour {

	private int materialNumber;

	public void awake() {
		materialNumber = 0;
	}

	public void setMaterial(bool bump, bool spec, bool tran) {
		materialNumber = 0;
		if (tran) {
			materialNumber += 4;
		}
		if (spec) {
			materialNumber += 2;
		} 
		if (bump) {
			materialNumber += 1;
		}
	}

	public int getMaterialNumber() {
		return materialNumber;
	}

	public string getMaterialString() {
		return getMaterialString (materialNumber);
	}

	public string getMaterialString(int number) {
		string s = "Legacy Shaders/";

		if (number > 3) {
			s += "Transparent/";
		}

		if (number % 2 == 1) {
			s += "Bumped ";
		} 

		if (number % 4 > 2) {
			s += "Specular";
		} else {
			s += "Diffuse";
		}

		return s;
	}
}
