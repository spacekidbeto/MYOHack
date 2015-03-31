using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallScript : MonoBehaviour {

	public Transform ObjectTransform;
	public VirtualUIBezierPath Bezier;
	public float TotalTime;
	public float InitialDelayTime = 0f;
	float StartTime;
	List<Vector3> bezierPoints;
	List<float> startingTimes = new List<float>();
	bool hasCalculated = false;

	// Use this for initialization
	void Start () {
		StartTime = TotalTime;
		if (Bezier.HasPathBeenCalculated) {
			CalculateBezierAnimation();
		}
		Bezier.PathCalculated += HandlePathCalculated;
	}

	void CalculateBezierAnimation() {
		bezierPoints = Bezier.GetBezierPoints ();
		startingTimes = new List<float> ();
		for (int z=0;z<bezierPoints.Count;z++) {
			startingTimes.Add((float)z / (float)bezierPoints.Count * TotalTime);
		}
		hasCalculated = true;
	}

	void HandlePathCalculated (object sender, System.EventArgs e)
	{
		CalculateBezierAnimation ();
	}
	
	// Update is called once per frame
	void Update () {
		if (hasCalculated)
		{
			if (InitialDelayTime > 0.0f) {
				InitialDelayTime -= Time.deltaTime;
				return;
			}

			TotalTime -= Time.deltaTime;
			if (TotalTime < 0f)
				TotalTime = StartTime;

			bool found = false;
			Vector3 position = Vector3.zero;
			for (int z=bezierPoints.Count - 1;z>=0;z--) {
				if (!found)
				{
					if (StartTime - TotalTime > startingTimes[z]) {
						found = true;
						position = bezierPoints[z];
					}
				}
			}

			if (found)
			{
				ObjectTransform.transform.localPosition = position;
			}
		}
	}
}
