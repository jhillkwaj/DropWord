using UnityEngine;
using System.Collections;

public class RandomLetter : MonoBehaviour {
	System.Random r = new System.Random ();
	public GameObject Letter;
	string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	public void set(int[] odds, int seed)
	{
		r = new System.Random (seed);
		string newText = "";
		int typeCount = 1;
		foreach(int f in odds)
		{
			typeCount+=f;
		}
		int letter = r.Next(1, typeCount);
		int findLetter = 0;
		int i = -1;
		//Debug.Log (letter);
		while(findLetter<letter) {
			i++;
			findLetter+=odds[i];

				}


		Letter.GetComponent<TextMesh>().text = letters.Substring(i,1);
	}
}
