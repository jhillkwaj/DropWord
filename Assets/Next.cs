using UnityEngine;
using System.Collections;

public class Next : MonoBehaviour {

	public int time;
	
	// Update is called once per frame
	void FixedUpdate () {
		if (--time < 0) {
			Application.LoadLevel (Application.loadedLevel+1);
				}
	}
}
