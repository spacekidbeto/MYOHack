using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

[ExecuteInEditMode]
public class VirtualUIPieChart : MonoBehaviour {

	public float Radius = 2f;
	public float HoleRadius = 1f;
	public int Segments = 128;
	public float Height = 2f;
	public float ExplodedRadius = 0.5f;
	public float TextRadius = 3f;
	public VirtualUIFadableText TextTemplate;

	public float FilletSegments = 6f;
	public float ChamferFilletLength = 0.25f;

	public bool UseText = true;

	Mesh pieChart = null;
	public MeshFilter PieChartFilter;
	public LineRenderer PieChartLineRenderer;

	public PieChartValueCollection PieChartValues;

	public EaseOutProperties Easing;
	Mesh _oldMesh = null;

	bool runThreadDone = false;
	bool runningChart = false;
	
	#if UNITY_WINRT || UNITY_WP8
	#else
	BackgroundWorker worker = new BackgroundWorker();
	#endif

	List<int> triangles = new List<int>();
	List<Vector3> verts = new List<Vector3>();
	List<Vector2> uvs = new List<Vector2>();
	List<Color> colors = new List<Color>();
	int subMeshes = 1;
	int [] subMeshStarts;
	int [] subMeshEnds;
	int [] subMeshMidpoints;
	Vector3 centerPosition;
	Vector3 [] innerMeshPoints;
	Vector3 [] outerMeshPoints;
	Vector3 [] innerMeshPointsFillet;
	Vector3 [] outerMeshPointsFillet;
	
	float chamferInternalSideLength;
	
	float halfHeight;
	float heightTop;
	float heightBottom;
	float heightTopFillet;
	float heightBottomFillet;
	
	List<Vector3> positions = new List<Vector3>();
	
	int vertex = 0;
	int currentMesh = 0;

	Dictionary<int, List<int>> subTriangles = new Dictionary<int, List<int>>();
	Vector3 [] subMeshExplodedRadius;


	//public Material PieChartMainMaterial;

	Vector3 PointAtRadiusAngle(float angle, float radius, Vector3 center)
	{
		return new Vector3(center.x + (radius * Mathf.Cos(angle * Mathf.Deg2Rad)), center.y + (radius * Mathf.Sin(angle * Mathf.Deg2Rad)), center.z);
	}

	// Use this for initialization
	void Start () {
		this.DrawPie();
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR
		if (!Application.isPlaying) {
			UnityEditor.EditorUtility.SetDirty(this);
			this.DrawPie();
		}
		#endif

		if (runThreadDone) {
			int totalSubs = 0;
			for (int z=0;z<subMeshes;z++) {
				//totalSubs += subTriangles[z].Count;
			}
			//if (verts.Count == uvs.Count && verts.Count == colors.Count && (verts.Count / 4) == (totalSubs / 6)) {
				runThreadDone = false;
				try 
				{
					/*
					pieChart.Clear();
					pieChart.subMeshCount = subMeshes;

					pieChart.vertices = verts.ToArray();
					pieChart.colors = colors.ToArray();
					for (int z=0;z<subMeshes;z++) {
						pieChart.SetTriangles(subTriangles[z].ToArray(),z);
					}
					pieChart.uv = uvs.ToArray();
					pieChart.RecalculateNormals();
					*/

					#if UNITY_EDITOR
					if (!Application.isPlaying) {
						//PieChartFilter.mesh = pieChart;
					}
					#endif
					/*
					if (Application.isPlaying) {
						if (this.Easing != null) {
							this.Easing.SetDuration(1f);
							this.Easing.SetRenderer(pieChart,this.PieChartFilter,_oldMesh);
							this.Easing.StartEasing();
							this._oldMesh = pieChart;
						}
						else {
							PieChartFilter.mesh = pieChart;
						}
					}
					*/
				}
				catch 
				{
					
				}
				runningChart = false;
			//}
		}
	}

	#if UNITY_WINRT || UNITY_WP8
	void DrawChart_DoWork (object sender, System.EventArgs e) {
	#else
	void DrawChart_DoWork(object sender, DoWorkEventArgs e) {
	#endif
		/*
		triangles = new List<int>();
		verts = new List<Vector3>();
		uvs = new List<Vector2>();
		colors = new List<Color>();
		vertex = 0;
		for (int z=0;z<this.Segments;z++)
		{
			float angle = (float)z / (float)this.Segments * 360.0f;
			innerMeshPoints[z] = this.PointAtRadiusAngle(angle, this.HoleRadius, centerPosition);
			outerMeshPoints[z] = this.PointAtRadiusAngle(angle, this.Radius, centerPosition);
			//innerMeshPointsFillet[z] = this.PointAtRadiusAngle(angle, this.HoleRadius + chamferInternalSideLength, centerPosition);
			//outerMeshPointsFillet[z] = this.PointAtRadiusAngle(angle, this.Radius - chamferInternalSideLength, centerPosition);

		}

		halfHeight = this.Height / 2.0f;
		heightTop = centerPosition.z + halfHeight;
		heightBottom = centerPosition.z - halfHeight;
		Vector3 addTop = new Vector3(0f,0f,halfHeight);
		Vector3 addBottom = new Vector3(0f,0f,-1f * halfHeight);

		subTriangles = new Dictionary<int, List<int>>();
		for (int z=0;z<subMeshes;z++) {
			subTriangles.Add(z, new List<int>());
		}
		*/
		this.PieChartValues.CalculateValues();

		int z=0;
		try
		{
		foreach(PieChartValue pie in this.PieChartValues.PieChartValues) {
			pie.RenderPie(subMeshStarts[z]*3.6f,subMeshEnds[z]*3.6f,this.Radius,this.HoleRadius,this.centerPosition,this.ExplodedRadius,this.Height);
			z++;
		}
		}
		catch(UnityException ex) {
			Debug.Log(ex.ToString());
		}

		runThreadDone = true;
	}

	public void DrawPie()
	{
		this.runningChart = true;
		pieChart = new Mesh();
		for (int z=this.transform.childCount-1;z>=0;z--) {
			Transform trans = this.transform.GetChild(z);
			if (trans.name.StartsWith("PieText_")) {
				if (Application.isPlaying)
				{
					VirtualUIFadableText text = (VirtualUIFadableText)trans.GetComponent(typeof(VirtualUIFadableText));
					text.KillOnFadeOut = true;
					text.StartFadeOut();
				}
				else
				{
					DestroyImmediate(trans.gameObject);
				}
			}
		}

		#if UNITY_WINRT || UNITY_WP8
		#else
		worker = new BackgroundWorker();
		worker.DoWork += new DoWorkEventHandler(DrawChart_DoWork);
		#endif

		centerPosition = new Vector3(0f,0f,0f);
		innerMeshPoints = new Vector3[this.Segments];
		outerMeshPoints = new Vector3[this.Segments];
		innerMeshPointsFillet = new Vector3[this.Segments];
		outerMeshPointsFillet = new Vector3[this.Segments];
		
		chamferInternalSideLength = Mathf.Sqrt(this.ChamferFilletLength / 2f);
		
		halfHeight = this.Height / 2.0f;
		heightTop = centerPosition.z + halfHeight;
		heightBottom = centerPosition.z - halfHeight;
		heightTopFillet = centerPosition.z + halfHeight - chamferInternalSideLength;
		heightBottomFillet = centerPosition.z - halfHeight + chamferInternalSideLength;

		currentMesh = 0;
		if (this.PieChartValues == null) {
			subMeshStarts = new int[1] {0};
			// My chosen way of handling midpoints is to not show exploded pie slices if
			// you have only one segment.
			subMeshMidpoints = new int[1] {0};
			subMeshExplodedRadius = new Vector3[1] {new Vector3(0f,0f,0f)};
			subMeshEnds = new int[1] {this.Segments-1};
		}
		else {
			this.PieChartValues.CalculateValues();
			subMeshStarts = new int[PieChartValues.UsedPercentages.Length];
			subMeshMidpoints = new int[PieChartValues.UsedPercentages.Length];
			subMeshExplodedRadius = new Vector3[PieChartValues.UsedPercentages.Length];
			subMeshEnds = new int[PieChartValues.UsedPercentages.Length];
			subMeshes = PieChartValues.UsedPercentages.Length;

			int currentPosition = 0;
			for (int z=0;z<PieChartValues.UsedPercentages.Length;z++) {

				subMeshStarts[z] = currentPosition;
				currentPosition += (int)PieChartValues.UsedPercentages[z] * (int)((float)this.Segments / 100.0f);
				subMeshEnds[z] = currentPosition;

				if (subMeshEnds[z] >= this.Segments) {
					subMeshEnds[z] = this.Segments;
				}
				/*
				if (z == PieChartValues.UsedPercentages.Length - 1) {
					subMeshEnds[z] = this.Segments;
				}
				*/

				//Debug.Log(subMeshStarts[z]+"/"+subMeshEnds[z]);
				//currentPosition++;
				subMeshMidpoints[z] = (subMeshStarts[z] + subMeshEnds[z]) / 2;

				float angle = (float)subMeshMidpoints[z] / (float)this.Segments * 360.0f;
				subMeshExplodedRadius[z] = this.PointAtRadiusAngle(angle, ExplodedRadius, centerPosition);

				TextAnchor anchor = TextAnchor.UpperLeft;
				TextAlignment alignment = TextAlignment.Left;
				if (angle == 0f) {
					anchor = TextAnchor.MiddleLeft;
					alignment = TextAlignment.Left;
				}
				else if (angle == 90f) {
					anchor = TextAnchor.LowerCenter;
					alignment = TextAlignment.Center;
				}
				else if (angle == 180f) {
					anchor = TextAnchor.MiddleRight;
					alignment = TextAlignment.Right;
				}
				else if (angle == 270f) {
					anchor = TextAnchor.UpperCenter;
					alignment = TextAlignment.Center;
				}
				else if (angle > 0f && angle < 90f) {
					anchor = TextAnchor.LowerLeft;
					alignment = TextAlignment.Left;
				}
				else if (angle > 90f && angle < 180f) {
					anchor = TextAnchor.LowerRight;
					alignment = TextAlignment.Right;
				}
				else if (angle > 180f && angle < 270f) {
					anchor = TextAnchor.UpperRight;
					alignment = TextAlignment.Right;
				}
				else if (angle > 270f && angle < 360f) {
					anchor = TextAnchor.UpperLeft;
					alignment = TextAlignment.Left;
				}

				Vector3 textPoint = this.PointAtRadiusAngle(angle, TextRadius + PieChartValues.PieChartValues[z].DataExtraWidth, centerPosition);
				Vector3 lastScale = this.TextTemplate.TextObject.transform.localScale;
				VirtualUIFadableText newMesh = (VirtualUIFadableText)Instantiate(this.TextTemplate);
				newMesh.TextObject.transform.rotation = this.transform.rotation;
				newMesh.TextObject.transform.position = this.transform.position;
				newMesh.TextObject.transform.Translate(textPoint);

				newMesh.TextObject.anchor = anchor;
				newMesh.TextObject.alignment = alignment;
				string formattedText = string.Format("{0}{3}{1:N2} ({2:N2}%)", 
				                                     PieChartValues.PieChartValues[z].DataName,
				                                     PieChartValues.PieChartValues[z].DataValue,
				                                     PieChartValues.Percentages[z],
				                                     System.Environment.NewLine);
				newMesh.TextObject.text = formattedText;
				newMesh.TextObject.transform.name = "PieText_"+z;
				newMesh.TextObject.transform.parent = this.gameObject.transform;

				if (Application.isPlaying) {
					if (UseText) {
						newMesh.StartFadeIn();
					}
					else {
						newMesh.TextObject.color = new Color(1f,1f,1f,0f);
					}
					
				}
			}


		}

		if (Application.isPlaying) {
			#if UNITY_WINRT || UNITY_WP8
			this.DrawChart_DoWork(this, new System.EventArgs());
			#else
			worker.RunWorkerAsync();
			#endif
		}
		else {
			#if UNITY_EDITOR
			this.DrawChart_DoWork(this, new DoWorkEventArgs(null));
			#endif
		}


		//PieChartFilter.mesh = pieChart;
	}
}
