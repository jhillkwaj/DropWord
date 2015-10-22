using UnityEngine;
using System.Collections;

public class BombRunner : MonoBehaviour {

	public int time;
	public bool active;

	public float destroyDist = 1;
	public float moveDist = 10;
	public float forceAmount = .5f;
	
	// Update is called once per frame
	void FixedUpdate () {
		if (this.gameObject.GetComponent<Drag> ().active) {
			time--;
			this.gameObject.GetComponent<Drag>().Letter.GetComponent<TextMesh>().text = ""+((time/60)+1);
				}

		if (time <= 0) {

			Collider2D[] arr = Physics2D.OverlapCircleAll (this.transform.position, destroyDist);
			foreach (Collider2D g in arr) {
				if (g.transform.gameObject.tag == "Tile") {
					Destroy(g.gameObject);
				}
			}


			Collider2D[] move = Physics2D.OverlapCircleAll (this.transform.position, moveDist);
			foreach (Collider2D g in move) {
				if (g.transform.gameObject.tag == "Tile") {
					AudioSource.PlayClipAtPoint((AudioClip)Resources.Load ("bomb"), GameObject.FindGameObjectWithTag("MainCamera").transform.position);
					float xForce;
					float yForce;
					if(this.transform.position.x<g.transform.position.x)
						xForce = moveDist/(forceAmount*(Mathf.Sqrt(Mathf.Abs(this.transform.position.x-g.transform.position.x))));
					else
						xForce = -1*(moveDist/(forceAmount*(Mathf.Sqrt(Mathf.Abs(g.transform.position.x-this.transform.position.x)))));
					if(this.transform.position.y<g.transform.position.y)
						yForce = moveDist/(forceAmount*(Mathf.Sqrt(Mathf.Abs(this.transform.position.y-g.transform.position.y))));
					else
						yForce = -1*(moveDist/(forceAmount*(Mathf.Sqrt(Mathf.Abs(g.transform.position.y-this.transform.position.y)))));
					g.rigidbody2D.AddForce(new Vector2(xForce,yForce));
				}
			}



			Destroy(this.gameObject);
				}

		if (time == 120) {
			AudioSource.PlayClipAtPoint((AudioClip)Resources.Load ("fuse"), GameObject.FindGameObjectWithTag("MainCamera").transform.position);
				}
	}


	void OnTriggerEnter2D(Collider2D trigger ) {
		if (this.GetComponent<Drag>().active&&trigger.gameObject.tag == "Tile") {          
			AudioSource.PlayClipAtPoint((AudioClip)Resources.Load ("metal"), GameObject.FindGameObjectWithTag("MainCamera").transform.position,.05f);

		}
	}
}
