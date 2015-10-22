using UnityEngine;
using System.Collections;

public class SaveCoins : MonoBehaviour {

	public static void saveCoins(int coinNum)
	{
		PlayerPrefs.SetInt("Number of Coins",coinNum);
		float coinFloat = (float)coinNum;

		coinFloat *= 172;
		coinFloat += 17;
		coinFloat *= 3;

		string coinString = ""+coinFloat;
		for (int i = 0; i < 759; i++) {
			coinString = "" + Random.Range (0, 10) + "" + coinString;
				}
		for (int i = 0; i < 123; i++) {
			coinString = coinString + "" + Random.Range (0, 10) + "";
		}
		Debug.Log (coinString.Length + " " +coinString);
		PlayerPrefs.SetString("Saved Level Data",coinString);
	}


	public static int getCoins()
	{
		if (!PlayerPrefs.HasKey ("Number of Coins")) {
			saveCoins(0);
				}


		int coins = PlayerPrefs.GetInt("Number of Coins");

		string level = PlayerPrefs.GetString("Saved Level Data");

		string coinsString = "" + (((coins*172)+17)*3);
		if(coins == ((int.Parse(level.Substring(759,coinsString.Length))/3)-17)/172)
		{
			return coins;
		}
		else
		{
			return 0;
		}
	}
}
