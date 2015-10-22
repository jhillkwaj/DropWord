using UnityEngine;
using System.Collections;

//using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class BuyCoins : MonoBehaviour {


	public int purchaseNumber;
	PurchasableItem[] items;
	void Start(){
	
		items = Unibiller.AllPurchasableItems;
	}



	void OnMouseDown(){

		Unibiller.initiatePurchase(items[purchaseNumber]);
		GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().checkCoins ();
	}





}