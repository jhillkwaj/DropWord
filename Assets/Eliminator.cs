using UnityEngine;
using System.Collections;

public class Eliminator : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D trigger ) {
		if (trigger.gameObject.tag == "Tile" && trigger.gameObject.GetComponent<Drag>().active) {          
			Destroy(trigger.gameObject);
			Destroy(this.gameObject);
		}
	}
}
