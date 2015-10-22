using UnityEngine;
using System.Collections;

public class HideLevel : MonoBehaviour {

	public string levelCheck;
	public GameObject hide;
	// Use this for initialization
	void Start () {
		if(levelCheck == "")
		{hide.gameObject.SetActive(false); return; }
		if(PlayerPrefs.HasKey("savedGame")){
			string level = PlayerPrefs.GetString("savedGame");


			if(level.Contains(" "+levelCheck+" "))
			{
				hide.gameObject.SetActive(false);
				return;
			}
		}
		Destroy (this.GetComponent<BoxCollider2D> ());

	}

}
