using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class FailRefrence : MonoBehaviour {

	// Use this for initialization
	public GameObject fail;

	int coins = 0;

	public int bombCost = 50;
	public int randomAllCost = 30;
	public int freeTileCost = 60;
	public int eliminateCost = 30;

	void Start()
	{
		startUnibill ();
		bombCost *= 3;
		randomAllCost *= 2;
		freeTileCost *= 7;
		eliminateCost *= 2;


		//coins = SaveCoins.getCoins();
		//Debug.Log (coins);
		//Debug.Log (Unibiller.GetCurrencyBalance ("Coins").ToString());
		coins = int.Parse( Unibiller.GetCurrencyBalance ("Coins").ToString());
		this.GetComponent<Camera> ().orthographicSize = 5.3f;
		this.transform.position = new Vector3 (0, .3f, -10);


		Time.timeScale = 1.0f;
		GameObject camera = (GameObject)Instantiate (Resources.Load ("UI Camera"));
		GameObject shopUI = (GameObject)Instantiate (Resources.Load ("ShopUI"));
		shopUI.transform.position = new Vector3 (0, 0, 0);
		camera.transform.position = this.transform.position;
		}

	public void checkCoins()
	{
		coins = int.Parse( Unibiller.GetCurrencyBalance ("Coins").ToString());
	}


	public  int getCoins()
	{
		return int.Parse( Unibiller.GetCurrencyBalance ("Coins").ToString());
	}

	public void addCoins(int addCoins)
	{
		Unibiller.CreditBalance("Coins", addCoins);
	}

	public int getBombCost()
	{
		return bombCost;
	}

	public int getRandomAllCost()
	{
		return randomAllCost;
	}

	public int getFreeTileCost()
	{
		return freeTileCost;
	}

	public int getEliminateCost()
	{
		return eliminateCost;
	}


	void startUnibill()
	{
		// We must first hook up listeners to Unibill's events.
		Unibiller.onBillerReady += onBillerReady;
		Unibiller.onTransactionsRestored += onTransactionsRestored;
		Unibiller.onPurchaseCancelled += onCancelled;
		Unibiller.onPurchaseFailed += onFailed;
		Unibiller.onPurchaseCompleteEvent += onPurchased;
		Unibiller.onDownloadProgressedEvent += (item, progress) => {
			Debug.Log(item.name + " " + progress);
		};
		
		Unibiller.onDownloadFailedEvent += (arg1, arg2) => {
			Debug.LogError(arg2);
		};
		
		Unibiller.onDownloadCompletedEvent += (obj, dir) => {
			Debug.Log("Completed download: " + obj.name);
			foreach (var f in dir.GetFiles()) {
				Debug.Log(f.Name);
				if (f.Name.EndsWith("txt") && f.Length < 10000) {
					Debug.Log(File.ReadAllText(f.FullName));
				}
			}
		};
		
		// Now we're ready to initialise Unibill.
		Unibiller.Initialise();
	}


	private void onBillerReady(UnibillState state) {
		UnityEngine.Debug.Log("onBillerReady:" + state);
	}
	
	private void onTransactionsRestored (bool success) {
		Debug.Log("Transactions restored.");
	}
	
	private void onPurchased(PurchaseEvent e) {
		Debug.Log("Purchase OK: " + e.PurchasedItem.Id);
		Debug.Log ("Receipt: " + e.Receipt);
		Debug.Log(string.Format ("{0} has now been purchased {1} times.",
		                         e.PurchasedItem.name,
		                         Unibiller.GetPurchaseCount(e.PurchasedItem)));
	}
	
	private void onFailed(PurchasableItem item) {
		Debug.Log("Purchase failed: " + item.Id);
	}
	
	private void onCancelled(PurchasableItem item) {
		Debug.Log("Purchase cancelled: " + item.Id);
	}

}
