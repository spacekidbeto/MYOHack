using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class VirtualUIBezierPath : MonoBehaviour {

	public BezierSegment [] Segments;
	public LineRenderer BezierRenderer;
	public int DetailSegments;
	public bool UpdateInEditor = true;
	BezierPath path = new BezierPath();
	public event System.EventHandler PathCalculated;
	public bool HasPathBeenCalculated = false;
	public bool DrawLineRenderer = false;

	// Use this for initialization
	void Start () {
		SetBezierPath ();
		if (Application.isPlaying) {
			if (!DrawLineRenderer) {
				BezierRenderer.enabled = false;
			}
		}
	}

	public void SetBezierPath() {
		if (Segments == null)
			return;
		List<Vector3> pointList = new List<Vector3> ();
		for (int z=0; z<Segments.Length; z++) {
			if (z == 0) {
				pointList.Add(Segments[z].StartPoint);
			}
			pointList.Add(Segments[z].FirstControlPoint);
			pointList.Add(Segments[z].SecondControlPoint);
			pointList.Add(Segments[z].EndPoint);
		}
		path.SetControlPoints (pointList);
		if (PathCalculated != null) {
			PathCalculated(this, new System.EventArgs());
		}
		HasPathBeenCalculated = true;
	}

	public List<Vector3> GetBezierPoints() {
		List<Vector3> list = new List<Vector3>();
		try
		{
		for (int y=0;y<Segments.Length;y++)
		{
			for (int z=0;z<DetailSegments;z++) {
				list.Add(path.CalculateBezierPoint(y, (float)z / (float)DetailSegments));
			}
		}
		list.Add(path.CalculateBezierPoint(Segments.Length - 1, 1f));
		}
		catch
		{
		}
		return list;
	}

	public void RedrawBezier() {
		if (BezierRenderer == null)
			return;
		if (DetailSegments > 0) {
			List<Vector3> list = GetBezierPoints();
			BezierRenderer.SetVertexCount(list.Count);
			for (int z=0;z<list.Count;z++) {
				BezierRenderer.SetPosition(z, list[z]);
			}
		} else {
			BezierRenderer.SetVertexCount(1);
			BezierRenderer.SetPosition(0, Vector3.zero);
		}
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR
		if (!Application.isPlaying) {
			if (UpdateInEditor) {
				SetBezierPath();
				RedrawBezier();
			}
		}
		#endif
	}
}
