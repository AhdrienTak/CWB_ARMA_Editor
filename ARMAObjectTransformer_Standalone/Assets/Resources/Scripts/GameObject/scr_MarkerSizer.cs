using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_MarkerSizer : MonoBehaviour {

	private int mark_w;
	private int mark_h;

	public void SetWidth(int w) {
		mark_w = w;
	}

	public void SetHeight(int h) {
		mark_h = h;
	}

	public void NormalizeScale(int w, int h) {
		SetWidth (w);
		SetHeight (h);
		NormalizeScale ();
	}

	public void NormalizeScale(){
		float scaleX;
		float scaleZ;

		if (mark_w >= mark_h) {
			scaleX = ((float)mark_w) / ((float)mark_h);
			scaleZ = 1.0f;
		} else {
			scaleX = 1.0f;
			scaleZ = ((float)mark_h) / ((float)mark_w);
		}

		this.transform.localScale = new Vector3(scaleX, 1.0f, scaleZ);
	}
}
