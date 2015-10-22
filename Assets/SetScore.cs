using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetScore : MonoBehaviour {
	bool showWin = false;
	public int wordsLeft;
	public string[] words;
	public string wordsFromList = "";
	public int[] points;
	public GameObject pointsGUI;

	public bool wordsPointsWin;

	public bool partsOfWords;

	public bool onlyOnce;

	public bool lengthPoints;

	public bool timeWin;

	public bool clearWin;

	public GameObject[] removeWin;

	public int lengthWin = -1;

	public GameObject win;
	
	public string winText;

	public int timeLimit = -1;
	public GameObject timeLimitText;

	public bool clearAfterPoints;
	public GameObject clearAfterText;

	public bool inARow;
	int rowNum = -1;
	int lastTileNum = -1;

	public List<string> previousWords = new List<string>();


	
	// Use this for initialization
	public bool score(string word){
		bool returnValue = false;
		if (wordsPointsWin) {
			int startPoints = wordsLeft;

						if (!partsOfWords) {
								word = word.ToLower ();
								for (int i = 0; i < words.Length; i++) {
										if (words [i] == word) {
												wordsLeft -= points [i];
												wordOnlyOnce (i);
						returnValue = true;
						previousWords.Add(word);
										}
								}
						}


						if (partsOfWords) {
								word = word.ToLower ();
								for (int i = 0; i < words.Length; i++) {
										if (word.Contains (words [i])) {
												wordsLeft -= points [i];
												wordOnlyOnce (i);
						returnValue =  true;
						previousWords.Add(word);
										}
								}
						}

			if(lengthWin>2)
			{
				if(word.Length>=lengthWin)
				{
					wordsLeft--;
					returnValue =  true;
					previousWords.Add(word);
				}
			}

			if(lengthPoints)
			{
				wordsLeft -= (word.Length-3)*(startPoints-wordsLeft);
			}



						pointsGUI.GetComponent<TextMesh> ().text = "" + wordsLeft;

						if (wordsLeft <= 0 && !showWin) {
							if(!clearAfterPoints)
								youWin();
				else
				{
					clearAfterPoints = false;
					wordsPointsWin = false;
					clearWin = true;
					clearAfterText.SetActive(true);
				}
						}
				}

		return returnValue;


	}

	void wordOnlyOnce(int i)
	{
		if (onlyOnce) {
			words[i]="";
			points[i]=0;
		}
	}

	void Update()
	{
		if (clearWin&& !showWin) {
			if (GameObject.FindGameObjectsWithTag ("Tile").Length + GameObject.FindGameObjectsWithTag ("Droppable").Length <= 4) {
				youWin ();
				
			}
			else
			{
				pointsGUI.GetComponent<TextMesh> ().text = "" + (GameObject.FindGameObjectsWithTag ("Tile").Length + GameObject.FindGameObjectsWithTag ("Droppable").Length-4);
			}
		}
		if (inARow) {
						int tileNum = GameObject.FindGameObjectsWithTag ("Tile").Length + GameObject.FindGameObjectsWithTag ("Droppable").Length;
						if (tileNum > lastTileNum) {
								wordsLeft = rowNum;
								pointsGUI.GetComponent<TextMesh> ().text = "" + wordsLeft;
				Debug.Log("Add Tile" + tileNum);
						} else if (lastTileNum > tileNum) {
								wordsLeft -= (lastTileNum-tileNum);
								pointsGUI.GetComponent<TextMesh> ().text = "" + wordsLeft;
				Debug.Log("Use Tile");
						}
						if (wordsLeft <= 0 && !showWin) {
								youWin ();
						}
			lastTileNum = tileNum;
				}
	}

	void FixedUpdate()
	{
				if (timeWin){
						wordsLeft--;
				if (wordsLeft <= 0 && !showWin) {
						youWin ();
				}
			pointsGUI.GetComponent<TextMesh> ().text = "" + ((wordsLeft/60)+1);
		}
		if (timeLimit > -1 && !showWin) {
						timeLimit--;
			timeLimitText.GetComponent<TextMesh>().text = "" + ((timeLimit/60)+1);
						if (timeLimit == 0) {
								Time.timeScale = 0;
								GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().fail.SetActive (true);
						}
				}
	}

	void Start()
	{
		if (wordsFromList!="") {
			TextAsset asset = (TextAsset)Resources.Load(wordsFromList);
			List<string> l = new List<string>();
			string textFromFile = asset.text;
			string[] words = textFromFile.Split('\n');
			
			for (int i = 0; i < words.Length; i++) {
				try{
					if(words[i].ToCharArray()[words[i].Length-2]!=' ')
					{
						l.Add(words[i].ToLower());
					}
					else
					{
					l.Add(words[i].Substring(0,words[i].Length-2).ToLower());
					}
				}
				catch(System.Exception e){ 
					Debug.Log(words[i]);
				}
			}
			this.words = new string[l.Count];
			for(int i = 0; i < l.Count; i++)
			{
				this.words[i]=l[i];
			}
				}
		if (inARow) {
			rowNum = wordsLeft;
				}
	}

	void youWin()
	{
		SaveCoins.saveCoins (GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().getCoins());
		AudioSource.PlayClipAtPoint((AudioClip)Resources.Load ("win"), GameObject.FindGameObjectWithTag("MainCamera").transform.position);
		win.SetActive (true);
		Time.timeScale = 0;
		showWin = true;
		if(PlayerPrefs.HasKey("savedGame"))
		{
			PlayerPrefs.SetString("savedGame",PlayerPrefs.GetString("savedGame")+" " + winText + " ");
		}
		else
		{PlayerPrefs.SetString("savedGame"," " + winText + " ");}
	}
}
