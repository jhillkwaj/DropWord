using UnityEngine;
using System.Collections;

public class ShopBuy : MonoBehaviour {

	public GameObject shop;

	public GameObject select;

	public GameObject costText;

	int cost = 999999;

	public bool bomb;
	public bool randomAll;
	public bool freeTile;
	public bool eliminate;
	public bool moreCoins;

	void Start()
	{
		if (bomb) {
						cost = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().getBombCost ();
				} else if (randomAll) {
						cost = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().getRandomAllCost ();
				} else if (freeTile) {
			            cost = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().getFreeTileCost();
				}
				else if (eliminate) {
						cost = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().getEliminateCost();
				}
		if(!moreCoins)
		costText.GetComponent<TextMesh> ().text = "" + cost;
	}


	void OnMouseDown()
	{
		if (moreCoins) {
			shop.SetActive(false);
			select.SetActive (true);
				}
		if (GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().getCoins () >= cost) {
						if (bomb||randomAll||freeTile||eliminate) {
								shop.SetActive(false);
								select.SetActive (true);
						}
				} else if(!moreCoins) {
			costText.GetComponent<TextMesh>().color = new Color(250,0,0);
			Debug.Log(cost + ">" + GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().getCoins ());
				}
	}



}
