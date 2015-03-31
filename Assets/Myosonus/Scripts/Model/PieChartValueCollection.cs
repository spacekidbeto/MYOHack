using UnityEngine;
using System.Collections;

public class PieChartValueCollection : MonoBehaviour {

	public PieChartValue [] PieChartValues;
	public float [] Percentages;
	public float [] UsedPercentages;

	public void CalculateValues() {
		float totalValue = 0f;
		Percentages = new float[PieChartValues.Length];
		UsedPercentages = new float[PieChartValues.Length];

		foreach(PieChartValue value in PieChartValues) {
			totalValue += value.DataValue;
		}

		int z = 0;
		foreach(PieChartValue value in PieChartValues) {
			Percentages[z] = value.DataValue / totalValue * 100.0f;
			z++;
		}
		// This next part is an algorithm I use to round down the 
		// percentages easier into pie chart segments. Basically I
		// round off the excess decimals from each pie segment and
		// attach to the last for the time being.
		float excess = 0f;
		float used = 0f;
		for (z=0;z<PieChartValues.Length;z++) {
			float currentExcess = Percentages[z] - Mathf.Floor(Percentages[z]);
			float actualUsed = Percentages[z] - currentExcess;
			used += actualUsed;
			UsedPercentages[z] = actualUsed;
			if (z == PieChartValues.Length - 1) {
				UsedPercentages[z] += 100f - used;
			}
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
