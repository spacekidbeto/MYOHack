using UnityEngine;
using System.Collections;

public class LinearChartValueCollection : MonoBehaviour {

	public LinearChartValue [] ChartValues;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public float MaximumValue {
		get {
			if (ChartValues == null) return 0f;
			float maxValue = ChartValues[0].DataValue;
			for (int z=0;z<ChartValues.Length;z++) {
				if (ChartValues[z].DataValue > maxValue)
					maxValue = ChartValues[z].DataValue;
			}
			return maxValue;
		}
	}

	public float MinimumValue
	{
		get {
			if (ChartValues == null) return 0f;
			float minValue = ChartValues[0].DataValue;
			for (int z=0;z<ChartValues.Length;z++) {
				if (ChartValues[z].DataValue < minValue)
					minValue = ChartValues[z].DataValue;
			}
			return minValue;
		}
	}
}
