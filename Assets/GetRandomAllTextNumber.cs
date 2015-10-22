using UnityEngine;
using System.Collections;

public class GetRandomAllTextNumber : MonoBehaviour {

	void Start () {
		this.GetComponent<TextMesh> ().text = "" + GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().randomAllCost;
	}
}
