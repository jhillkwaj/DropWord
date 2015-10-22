using UnityEngine;
using System.Collections;

public class GoingDown : MonoBehaviour {

	public bool stay =  false;
	public bool timeSpin = false;




	void Update()
	{
		if (timeSpin) {
						Vector3 rot = this.transform.rotation.eulerAngles;
						this.transform.rotation = Quaternion.Euler (rot.x, rot.y - 1, rot.z);
				}

	}

	
	// Update is called once per frame
	void FixedUpdate () {


		Vector3 rot = this.transform.rotation.eulerAngles;
		this.transform.rotation = Quaternion.Euler (rot.x, rot.y - 1, rot.z);
		if (!stay) {
						this.transform.position = new Vector3 (this.transform.position.x, this.transform.position.y + .01f, this.transform.position.z);
						if (Random.value < .005f) {
								Destroy (this.gameObject);
						}
				}
	}

	void spin()
	{
		Vector3 rot = this.transform.rotation.eulerAngles;
		this.transform.rotation = Quaternion.Euler (rot.x, rot.y - 5, rot.z);
	}
}
