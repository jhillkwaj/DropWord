using UnityEngine;
using System.Collections;

public class Drag : MonoBehaviour {

	public bool selected = false;
	public GameObject Letter;
	public bool active = false;

	int stayCount = 100;
	

	public float failLine;

	public int dropOdds = -1;
	bool dropSpawn = false;
	int dropSpawnCount = 20;
	Vector3 dropSpawnPos;

	System.Random r = null;


	
	// Update is called once per frame
	void Update () {
		if (dropOdds>0 && r == null) {
			r = new System.Random(GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RandomInt>().random());
			dropSpawnPos=this.transform.position;
				}
		try{
		if ((Input.GetMouseButtonUp (0) || Input.GetTouch (0).phase == TouchPhase.Ended)) {
			if(selected)
			{
				selected=!selected;
			    active = true;
				this.GetComponent<Rigidbody2D>().isKinematic=false;
			}

				}
		}
		catch(UnityException e)
		{
				}

		if (active&&this.transform.position.y>failLine&&rigidbody2D.velocity.magnitude<.1f&&Time.timeScale>0) {
			stayCount--;
				}

		if (stayCount < 0) {
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FailRefrence>().fail.SetActive(true);
			Time.timeScale = 0;
			if(stayCount>-10)
			{
			SaveCoins.saveCoins (GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().getCoins());
				stayCount=-20;
			}
				}

	}

	void FixedUpdate()
	{
		if (!active&&dropOdds > 1&&r!=null) {
			if(r.Next(1,dropOdds+1)==2)
			{
				selected=false;
				active = true;
				this.GetComponent<Rigidbody2D>().isKinematic=false;
				dropSpawn=true;
			}
				}
		if (dropSpawn) {
			if(dropSpawnCount<=0)
			{
				GameObject.FindGameObjectWithTag("Runner").GetComponent<Spawn>().newBlock(dropSpawnPos);
				dropSpawn = false;
			}
			else
			{
				dropSpawnCount--;
			}
				}
	}
}
