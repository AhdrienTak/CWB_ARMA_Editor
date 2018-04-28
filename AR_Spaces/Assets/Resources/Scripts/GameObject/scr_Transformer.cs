using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Transformer : MonoBehaviour {

	public void SetTransforms(string[] new_transforms) {
		this.transform.position = new Vector3(float.Parse(new_transforms[0]), float.Parse(new_transforms[1]), float.Parse(new_transforms[2]));
		this.transform.eulerAngles = new Vector3(float.Parse(new_transforms[3]), float.Parse(new_transforms[4]), float.Parse(new_transforms[5]));
		this.transform.localScale = new Vector3(float.Parse(new_transforms[6]), float.Parse(new_transforms[7]), float.Parse(new_transforms[8]));
	}
}
