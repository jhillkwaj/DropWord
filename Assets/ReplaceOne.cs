using UnityEngine;
using System.Collections;

public class ReplaceOne : MonoBehaviour {

	public int cost = 100;
	public bool TimeBomb;
	public bool freeTile;
	public bool eliminator;

	void Start()
	{
		if(TimeBomb)
			cost = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().getBombCost();
		else if(freeTile)
			cost = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().getFreeTileCost();
		else if (eliminator)
			cost = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().getEliminateCost();

	}

	void Update()
	{
			try {
				if ((Input.GetMouseButtonDown (0) || Input.GetTouch (0).phase == TouchPhase.Began)) {
					Vector3 clickedPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
					Collider2D[] arr = Physics2D.OverlapCircleAll (clickedPosition, .05f);
					foreach (Collider2D g in arr) {
						if (g.transform.gameObject.tag == "Tile" || g.transform.gameObject.tag == "Droppable") {
							if(TimeBomb)
						{
							GameObject bomb = (GameObject)Instantiate (Resources.Load ("TimeBomb"));
							bomb.transform.position = g.transform.gameObject.transform.position;
							Destroy (g.gameObject);
							Time.timeScale = 1;
							this.gameObject.SetActive (false);
							GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().addCoins(-cost);
							return;
						}
						else if(freeTile)
						{
							GameObject block = (GameObject)Instantiate (Resources.Load ("Block"));
							block.transform.position = g.transform.gameObject.transform.position;
							block.GetComponent<Drag>().Letter.GetComponent<TextMesh>().text = "*";
							Destroy (g.gameObject);
							Time.timeScale = 1;
							this.gameObject.SetActive (false);
							GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().addCoins(-cost);
							return;
						}
						else if(eliminator)
						{
							GameObject block = (GameObject)Instantiate (Resources.Load ("Eliminator"));
							block.transform.position = g.transform.gameObject.transform.position;
							Destroy (g.gameObject);
							Time.timeScale = 1;
							this.gameObject.SetActive (false);
							GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().addCoins(-cost);
							return;
						}
						}
					}
				}
			} catch (UnityException e) {
			}
	}
}
