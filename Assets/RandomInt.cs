using UnityEngine;
using System.Collections;

public class RandomInt : MonoBehaviour {

	System.Random r = new System.Random();
	// Use this for initialization
	public int random()
	{
		return r.Next (1, 1000000);
	}
}
