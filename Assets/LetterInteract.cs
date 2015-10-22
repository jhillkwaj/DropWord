using UnityEngine;
using System.Collections;

public class LetterInteract : MonoBehaviour {

	public ArrayList touchingTiles = new ArrayList();
	public Sprite defalt;
	public Sprite selected;

	public bool sticky;



	void OnTriggerEnter2D(Collider2D trigger ) {
		if (trigger.gameObject.tag == "Tile") {          
						touchingTiles.Add (trigger.gameObject);
		


				}
	}
	
	void OnTriggerExit2D(Collider2D trigger) {
				if (trigger.gameObject.tag == "Tile" && touchingTiles.Contains (trigger.gameObject)) {          
						touchingTiles.Remove (trigger.gameObject);

		

				}
		}

	public bool contains(GameObject g)
	{
		return touchingTiles.Contains(g);
	}
}
