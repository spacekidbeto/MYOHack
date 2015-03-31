using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

[ExecuteInEditMode]
public class VirtualUIAreaChart : MonoBehaviour {

	BezierPath pathFront = new BezierPath();
	BezierPath pathBack = new BezierPath();
	public bool IsSmooth = true;
	public int SmoothSegments = 5;
	public float SpaceBetweenDataElements = 0.5f;
	public float MaxHeight = 4f;
	public float MaxDepth = 2f;
	public float DataTextHeight = 0.2f;
	public float BottomPadding = 0.1f;
	public VirtualUIFadableText TextTemplate;
	public LinearChartValueCollection ChartData;
	public MeshFilter ChartFilter;
	public LineRenderer ChartLineRenderer;
	public EaseOutProperties Easing;
	Mesh chartMesh = null;
	Mesh _oldMesh = null;
	List<Vector3> vertices = new List<Vector3>();
	List<Vector2> uvs = new List<Vector2>();
	List<Color> colors = new List<Color>();
	List<int> triangles = new List<int>();
	bool runThreadDone = false;
	bool runningChart = false;

	#if UNITY_WINRT || UNITY_WP8
	#else
	BackgroundWorker worker = new BackgroundWorker();
	#endif

	// Use this for initialization
	void Start () {
		this.DrawChart();
	}

	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR
		if (!Application.isPlaying) {
			UnityEditor.EditorUtility.SetDirty(this);
			if (!runningChart) {
				this.DrawChart();
			}
		}
		#endif

		if (runThreadDone) {
			if (vertices.Count == uvs.Count && vertices.Count == colors.Count && (vertices.Count / 4) == (triangles.Count / 6)) {

				runThreadDone = false;
				try
				{
					chartMesh.Clear();
					chartMesh.vertices = vertices.ToArray();
					chartMesh.triangles = triangles.ToArray();
					chartMesh.uv = uvs.ToArray();
					chartMesh.colors = colors.ToArray();
					chartMesh.RecalculateNormals();
					
					#if UNITY_EDITOR
					if (!Application.isPlaying) {
						ChartFilter.mesh = chartMesh;
					}
					#endif
					if (Application.isPlaying) {
						if (this.Easing != null) {
							this.Easing.SetDuration(1f);
							this.Easing.SetRenderer(chartMesh,this.ChartFilter,_oldMesh);
							this.Easing.StartEasing();
							this._oldMesh = chartMesh;
						}
						else {
							ChartFilter.mesh = chartMesh;
						}
					}
				}
				catch
				{

				}
				runningChart = false;
			}
		}
	}

	#if UNITY_WINRT || UNITY_WP8
	void DrawChart_DoWork (object sender, System.EventArgs e) {
	#else
	void DrawChart_DoWork(object sender, DoWorkEventArgs e) {
	#endif
		float maxValue = ChartData.MaximumValue;
		float minValue = ChartData.MinimumValue;

		float halfDepth = this.MaxDepth / 2f;
		float halfWidth = this.SpaceBetweenDataElements / 2f;
		
		int currentPoint = 0;

		Vector3[,] dataPoints = new Vector3[ChartData.ChartValues.Length,SmoothSegments];
		for (int z=0;z<ChartData.ChartValues.Length;z++) {
			float currentHeightPercent = (ChartData.ChartValues[z].DataValue - minValue) / (maxValue - minValue);
			float currentHeightValue = Mathf.Lerp(0f,this.MaxHeight,currentHeightPercent);
			Vector3 currentPositionVector = new Vector3(z * SpaceBetweenDataElements, currentHeightValue + this.BottomPadding, halfDepth);
			dataPoints[z,0] = currentPositionVector;
		}
		List<Vector3> linearPoints = new List<Vector3>();
		for (int z=0;z<ChartData.ChartValues.Length - 1;z++) {
			Vector3 firstPoint = dataPoints[z,0];
			Vector3 secondPoint = dataPoints[z+1,0];
			Vector3 firstHalfPoint = firstPoint + new Vector3(halfWidth,0f,0f);
			Vector3 secondHalfPoint = secondPoint + new Vector3(-1f * halfWidth,0f,0f);
			
			List<Vector3> vectors = new List<Vector3>() {
				firstPoint, firstHalfPoint, secondHalfPoint, secondPoint
			};
			pathFront.SetControlPoints(vectors);
			for (int y=0;y<SmoothSegments;y++) {
				dataPoints[z,y] = pathFront.CalculateBezierPoint(0, (float)y / (float)this.SmoothSegments);
			}
		}
		for (int z=0;z<ChartData.ChartValues.Length - 1;z++) {
			for (int y=0;y<SmoothSegments;y++) {
				linearPoints.Add(dataPoints[z,y]);
			}
		}
		linearPoints.Add(dataPoints[ChartData.ChartValues.Length - 1,0]);
		
		currentPoint = 0;
		for (int z=0;z<linearPoints.Count-1;z++) {
			// FRONT FACES
			Vector3 cp = linearPoints[z];
			Vector3 np = linearPoints[z+1];
			Vector3 czp = cp;
			czp.y = 0f;
			Vector3 nzp = np;
			nzp.y = 0f;
			
			float startUv = (z == 0) ? 0f : 1f / (float)z;
			float endUv = (z+1 == 0) ? 0f : 1f / (float)(z+1);
			
			vertices.AddRange(new Vector3[] {cp,np,czp,nzp});
			vertices.AddRange(new Vector3[] {cp,np,czp,nzp});
			int[] forwardQuad = {currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
			currentPoint += 4;
			int[] reverseQuad = {currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
			currentPoint += 4;
			
			triangles.AddRange(forwardQuad);
			triangles.AddRange(reverseQuad);
			uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
			uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
			colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			
			// BACK FACES
			Vector3 cpB = linearPoints[z];
			cpB.z = -1f * halfDepth;
			Vector3 npB = linearPoints[z+1];
			npB.z = -1f * halfDepth;
			Vector3 czpB = cpB;
			czpB.y = 0f;
			Vector3 nzpB = npB;
			nzpB.y = 0f;
			
			vertices.AddRange(new Vector3[] {cpB,npB,czpB,nzpB});
			vertices.AddRange(new Vector3[] {cpB,npB,czpB,nzpB});
			forwardQuad = new int[] {currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
			currentPoint += 4;
			reverseQuad = new int[] {currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
			currentPoint += 4;
			
			triangles.AddRange(forwardQuad);
			triangles.AddRange(reverseQuad);
			uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
			uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
			colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			
			// TOP FACES
			vertices.AddRange(new Vector3[] {cp,np,cpB,npB});
			vertices.AddRange(new Vector3[] {cp,np,cpB,npB});
			forwardQuad = new int[] {currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
			currentPoint += 4;
			reverseQuad = new int[] {currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
			currentPoint += 4;
			
			triangles.AddRange(forwardQuad);
			triangles.AddRange(reverseQuad);
			uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
			uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
			colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			
			// BOTTOM FACES
			vertices.AddRange(new Vector3[] {czp,nzp,czpB,nzpB});
			vertices.AddRange(new Vector3[] {czp,nzp,czpB,nzpB});
			forwardQuad = new int[] {currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
			currentPoint += 4;
			reverseQuad = new int[] {currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
			currentPoint += 4;
			
			triangles.AddRange(forwardQuad);
			triangles.AddRange(reverseQuad);
			uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
			uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
			colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			
			if (z == 0) {
				// FIRST FACE
				vertices.AddRange(new Vector3[] {cp,cpB,czp,czpB});
				vertices.AddRange(new Vector3[] {cp,cpB,czp,czpB});
				forwardQuad = new int[] {currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[] {currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			}
			else if (z == linearPoints.Count - 2) {
				// LAST FACE
				vertices.AddRange(new Vector3[] {np,npB,nzp,nzpB});
				vertices.AddRange(new Vector3[] {np,npB,nzp,nzpB});
				forwardQuad = new int[] {currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[] {currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,startUv), new Vector2(0f,endUv), new Vector2(1f,startUv), new Vector2(1f,endUv)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			}
		}
		runThreadDone = true;
	}

	public void DrawChart() {
		runningChart = true;
		for (int z=this.transform.childCount-1;z>=0;z--) {
			Transform trans = this.transform.GetChild(z);
			if (trans.name.StartsWith("ChartText_")) {
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

		chartMesh = new Mesh();

#if UNITY_WINRT || UNITY_WP8
#else
		worker = new BackgroundWorker();
		worker.DoWork += new DoWorkEventHandler(DrawChart_DoWork);
#endif

		float maxValue = ChartData.MaximumValue;
		float minValue = ChartData.MinimumValue;

		this.ChartLineRenderer.SetVertexCount((ChartData.ChartValues.Length - 1) * this.SmoothSegments);
		float halfDepth = this.MaxDepth / 2f;
		float halfWidth = this.SpaceBetweenDataElements / 2f;

		int currentPoint = 0;

		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		colors = new List<Color>();
		triangles = new List<int>();

		for (int z=0;z<ChartData.ChartValues.Length;z++) {
			float currentHeightPercent = (ChartData.ChartValues[z].DataValue - minValue) / (maxValue - minValue);
			float currentHeightValue = Mathf.Lerp(0f,this.MaxHeight,currentHeightPercent);
			Vector3 currentPositionVector = new Vector3(z * SpaceBetweenDataElements, currentHeightValue + this.BottomPadding, halfDepth);

			Vector3 textPositionVector = currentPositionVector;
			textPositionVector.z = 0f;
			textPositionVector.y += this.DataTextHeight;
			Vector3 textBottomPositionVector = textPositionVector;
			textBottomPositionVector.y = 0f - this.DataTextHeight;

			VirtualUIFadableText newText = (VirtualUIFadableText)Instantiate(this.TextTemplate);
			newText.TextObject.anchor = TextAnchor.LowerCenter;
			newText.TextObject.alignment = TextAlignment.Center;
			newText.TextObject.text = ChartData.ChartValues[z].DataValue.ToString("N2");
			newText.TextObject.transform.position = this.transform.position;
			newText.TextObject.transform.rotation = this.transform.rotation;
			newText.TextObject.transform.Translate(textPositionVector);

			VirtualUIFadableText newTextBottom = (VirtualUIFadableText)Instantiate(this.TextTemplate);
			newTextBottom.TextObject.anchor = TextAnchor.MiddleLeft;
			newTextBottom.TextObject.alignment = TextAlignment.Left;
			newTextBottom.TextObject.text = ChartData.ChartValues[z].DataName;
			newTextBottom.TextObject.transform.position = this.transform.position;
			newTextBottom.TextObject.transform.rotation = this.transform.rotation;
			newTextBottom.TextObject.transform.Translate(textBottomPositionVector);
			newTextBottom.TextObject.transform.Rotate(new Vector3(0f,0f,300f));
			Vector3 scale = newTextBottom.TextObject.transform.localScale;
			scale.x *= 0.75f;
			scale.y *= 0.75f;
			newTextBottom.TextObject.transform.localScale = scale;

			newText.name = "ChartText_" + z + "_Value";
			newTextBottom.name = "ChartText_" + z + "_Name";

			newText.transform.parent = this.transform;
			newTextBottom.transform.parent = this.transform;

			if (Application.isPlaying) {
				newText.StartFadeIn();
				newTextBottom.StartFadeIn();
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
	}
}
