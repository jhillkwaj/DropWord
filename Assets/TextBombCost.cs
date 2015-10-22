using UnityEngine;
using System.Collections;

public class TextBombCost : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.GetComponent<TextMesh> ().text = "" + GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().bombCost;
	}
	

}
