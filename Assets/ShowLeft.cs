using UnityEngine;
using System.Collections;

public class ShowLeft : MonoBehaviour {
	public int wordNum;
	public bool showWordsMade;

	// Update is called once per frame
	void Update () {
		if (!showWordsMade)
						this.gameObject.GetComponent<TextMesh> ().text = GameObject.FindGameObjectWithTag ("Runner").GetComponent<SetScore> ().words [wordNum].ToUpper ();
				else {
			if(GameObject.FindGameObjectWithTag ("Runner").GetComponent<SetScore> ().previousWords.Count>wordNum)
			this.gameObject.GetComponent<TextMesh> ().text = GameObject.FindGameObjectWithTag ("Runner").GetComponent<SetScore> ().previousWords [wordNum].ToUpper ();

				}
	}
}
