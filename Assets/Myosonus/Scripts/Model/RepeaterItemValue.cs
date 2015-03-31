using UnityEngine;
using System.Collections;

public class RepeaterItemValue : MonoBehaviour {

	public string PropertyName;
	public string PropertyValue;

	public enum PropertyTypes {
		String,
		Int,
		Float,
		DateTime
	}

	public PropertyTypes PropertyType;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
