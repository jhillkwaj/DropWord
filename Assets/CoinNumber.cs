using UnityEngine;
using System.Collections;

public class CoinNumber : MonoBehaviour {

	public GameObject mainCamera;

	void Start()
	{
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
	}

	void Update () {
		this.GetComponent<TextMesh> ().text = "" + mainCamera.GetComponent<FailRefrence> ().getCoins ();
	}
}
