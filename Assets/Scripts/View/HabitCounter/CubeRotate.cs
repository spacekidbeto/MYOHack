using UnityEngine;
using System.Collections;

public class CubeRotate : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.Rotate (Vector3.up, 50f * Time.deltaTime);
		this.transform.Rotate (Vector3.right, 50f * Time.deltaTime);
		this.transform.Rotate (Vector3.forward, 50f * Time.deltaTime);
	}
}
