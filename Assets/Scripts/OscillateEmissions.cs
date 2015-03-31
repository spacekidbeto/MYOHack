using UnityEngine;
using System.Collections;

public class OscillateEmissions : MonoBehaviour {

	public float Coefficient = 1f;
	public Material EmissionMaterial;
	Color startColor = Color.white;
	// Use this for initialization
	void Start () {
		startColor = EmissionMaterial.GetColor ("_EmissionColor");
	}
	
	// Update is called once per frame
	void Update () {
		float x = Time.timeSinceLevelLoad;
		float sineX = Coefficient * Mathf.Sin (x);

		Color col = startColor;
		col.a = sineX;
		//EmissionMaterial.SetColor ("_EmissionColor", col);

		//EmissionMaterial.SetFloat ("_EmissionScaleUI", sineX);
		//EmissionMaterial.GetFloat ("_EmissionScaleUI"));
		print (sineX);
	}
}
