using UnityEngine;
using System.Collections;

public class Destroy : MonoBehaviour {

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.name == "Destroy2D Mesh") {
			Destroy2D terrain = GameObject.Find("Destroy2D Object").GetComponent<Destroy2D>();
			terrain.destroyAt(transform.position, 3, 0.5f);
			Destroy(this);
		}
	}
}