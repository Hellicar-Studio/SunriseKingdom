using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

	void Update () {
	    transform.Rotate(0,Time.deltaTime * 20,0);
	}
}
