using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class Dictionary : MonoBehaviour {

	List<string> t = new List<string>();
	string letters = "abcdefghijklmnopqrstuvwxyz";
	public string lastWord;

	// Use this for initialization
	void Start () {
		TextAsset asset = (TextAsset)Resources.Load("words3");
		
		string textFromFile = asset.text;
		string[] words = textFromFile.Split('\n');

		for (int i = 0; i < words.Length; i++) {
			try{
				if(words[i].ToCharArray()[words[i].Length-2]!=' ')
				{
					t.Add(words[i].ToLower());
				}
				else
				{
					t.Add(words[i].Substring(0,words[i].Length-2).ToLower());
				}
			}
			catch(System.Exception e){ Debug.Log(words[i]);}
				}
	
		Debug.Log (words.Length);
		Debug.Log (t.Count);
		}

	public bool contains(string s)
	{
		if (s.Contains ("*")) {
			bool word = false;
			bool points = false;
			int lettersTried = 0;
			while(!points && lettersTried<26)
			{
				bool thisWord = false;
				string st = s;
				int index = s.IndexOf("*");
				st = st.Remove(index,1);
				st = st.Insert(index,letters.Substring(lettersTried,1));
				thisWord = this.contains(st);
				if(thisWord)
				{
					points = this.GetComponent<SetScore>().score(st);
				}
				if(!word){
					word = thisWord;
				}
				lettersTried++;
			}
			return word;
		}

	
		for(int i = 0; i < t.Count;i++)
		{

			if(s.Equals(t[i].ToString()))
			{
				lastWord = s;
				return true;
			}

		}
		return false;
	}
	

}
