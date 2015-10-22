using UnityEngine;
using System.Collections;

public class EliminateCost : MonoBehaviour {


	void Start () {
		this.GetComponent<TextMesh> ().text = "" + GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().eliminateCost;
	}
	

}
