using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour {
	System.Random r = new System.Random ();
	public int[] blockTypes;
	public bool lettersFromWords;
	string lettersOrder = "abcdefghijklmnopqrstuvwxyz";


	//block
	//circle;
	//pink block
	//pink circe
	//fast drop block
	//fast drop circle
	//slow drop block
	//slow drop circle
	//bomb
	public int[] letters = new int[26];

	// Use this for initialization


	public void newBlock(Vector3 spot){
		if (lettersFromWords) {
			setLetters();
				}


		int typeCount = 0;
		foreach(int f in blockTypes)
		{
			typeCount+=f;
		}
		int type = r.Next (1, typeCount+1);
		if (type <= blockTypes [0]) {
						GameObject block = (GameObject)Instantiate (Resources.Load ("Block"));
						block.transform.position = spot;
						block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
				} 
		type -= blockTypes [0];
		if (type <= blockTypes [1]) {
					GameObject block = (GameObject)Instantiate (Resources.Load ("Circle"));
					block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
				}
		type -= blockTypes [1];
		if (type <= blockTypes [2]) {
			GameObject block = (GameObject)Instantiate (Resources.Load ("BounceBlock"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
		}
		type -= blockTypes [2];
		if (type <= blockTypes [3]) {
			GameObject block = (GameObject)Instantiate (Resources.Load ("BounceCircle"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
		}
		type -= blockTypes [3];
		if (type <= blockTypes [4]) {
			GameObject block = (GameObject)Instantiate (Resources.Load ("BlockRedFast"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
		}
		type -= blockTypes [4];
		if (type <= blockTypes [5]) {
			GameObject block = (GameObject)Instantiate (Resources.Load ("RedCircleFast"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
		}
		type -= blockTypes [5];
		if (type <= blockTypes [6]) {
			GameObject block = (GameObject)Instantiate (Resources.Load ("BlockRedSlow"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
		}
		type -= blockTypes [6];
		if (type <= blockTypes [7]) {
			GameObject block = (GameObject)Instantiate (Resources.Load ("RedCircleSlow"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
		}
		type -= blockTypes [7];
		if (type <= blockTypes [8]) {
			GameObject block = (GameObject)Instantiate (Resources.Load ("TimeBomb"));
			block.transform.position = spot;
			return;
		}
		type -= blockTypes [8];
		if (type <= blockTypes [9]) {
			GameObject block = (GameObject)Instantiate (Resources.Load ("OvalBlue"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			return;
		}
		type -= blockTypes [9];
		if (type <= blockTypes [10]) {
			int[] setLetters = letters;
			this.setLetters();
			GameObject block = (GameObject)Instantiate (Resources.Load ("Block"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			letters = setLetters;
			return;
		} 
		type -= blockTypes [10];
		if (type <= blockTypes [11]) {
			int[] setLetters = letters;
			this.setLetters();
			GameObject block = (GameObject)Instantiate (Resources.Load ("Circle"));
			block.transform.position = spot;
			block.GetComponent<RandomLetter> ().set (letters, r.Next());
			letters = setLetters;
			return;
		}

	}

	public void setLetters()
	{
		letters = new int[26];
		foreach (string s in this.gameObject.GetComponent<SetScore>().words) {
			for(int i = 0; i < s.Length; i++)
			{
				try{
				letters[lettersOrder.IndexOf(s.Substring(i,1))]+=1;
				}catch(System.Exception e){}
			}
				}
	}
}
