using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour {

	public Vector3 loc;
	public int time;
	int timeLeft;
	public bool moveing;

	void Start()
	{
		Time.timeScale = 1;
	}
	// Update is called once per frame
	void FixedUpdate () {
		if(moveing&&timeLeft>0)
		{
			transform.position=new Vector3(transform.position.x+((loc.x-transform.position.x)/timeLeft),transform.position.y+((loc.y-transform.position.y)/timeLeft),transform.position.z+((loc.z-transform.position.z)/timeLeft));
			timeLeft--;
		}
	}
	
	public void startMove() {
		moveing = true;
		timeLeft = time;
	}
}
