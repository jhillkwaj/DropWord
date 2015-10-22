using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour {
	public Vector3 Loc;
	public Vector3 putLoc;
	public int time;
	void OnMouseDown() {
		if(putLoc.z!=0||putLoc.y!=0||putLoc.x!=0)
			GameObject.FindGameObjectWithTag ("MainCamera").transform.position = putLoc;
		GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<MoveCamera> ().loc = Loc;
		GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<MoveCamera> ().time = time;
		GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<MoveCamera> ().startMove();
		}
}
