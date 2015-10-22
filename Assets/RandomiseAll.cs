using UnityEngine;
using System.Collections;

public class RandomiseAll : MonoBehaviour {

	// Use this for initialization
	void OnMouseDown()
	{
		GameObject[] tiles = GameObject.FindGameObjectsWithTag ("Tile");

		foreach (GameObject g in tiles) {
			if(!g.GetComponent<Drag>().active)
			{
				GameObject.FindGameObjectWithTag("Runner").GetComponent<Spawn>().newBlock(g.transform.position);
				Destroy(g);
			}

				}
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().addCoins(-GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().randomAllCost);
		Time.timeScale = 1;
	}
}
