using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

[ExecuteInEditMode]
public class VirtualUIBarChart : MonoBehaviour {

	public enum BarChartType
	{
		Block,
		Cylinder,
		Pyramid
	}

	public float SpaceBetweenDataElements = 0.5f;
	public float MaxHeight = 4f;
	public float MaxDepth = 2f;
	public float ElementRadius = 0.5f;
	public float DataTextHeight = 0.2f;
	public float BottomPadding = 0.1f;
	public int CylinderSegments = 8;
	public BarChartType ChartType = BarChartType.Block;
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
		
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		colors = new List<Color>();
		triangles = new List<int>();
		
		float halfRadius = ElementRadius / 2f;
		float xposition = 0f;
		int currentPoint = 0;

		xposition = 0f;
		for (int z=0;z<ChartData.ChartValues.Length;z++) {
			xposition += halfRadius;
			
			float currentHeightPercent = (ChartData.ChartValues[z].DataValue - minValue) / (maxValue - minValue);
			float currentHeightValue = Mathf.Lerp(0f,this.MaxHeight,currentHeightPercent);
			Vector3 currentPositionVector = new Vector3(xposition, currentHeightValue + this.BottomPadding, 0f);
			Vector3 currentPositionVectorBottom = new Vector3(xposition, 0f, 0f);
			Vector3 radiusAdd = new Vector3(halfRadius,0f,0f);
			Vector3 depthAdd = new Vector3(0f,0f,halfDepth);
			
			// Right now I only support blocks and pyramids
			if (this.ChartType == BarChartType.Block) {
				// Top Face
				Vector3 tlb = currentPositionVector - radiusAdd - depthAdd;
				Vector3 trb = currentPositionVector + radiusAdd - depthAdd;
				Vector3 tlf = currentPositionVector - radiusAdd + depthAdd;
				Vector3 trf = currentPositionVector + radiusAdd + depthAdd;
				
				vertices.AddRange(new Vector3[] {tlb,trb,tlf,trf});
				vertices.AddRange(new Vector3[] {tlb,trb,tlf,trf});
				int[] forwardQuad = {currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				int[] reverseQuad = {currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				// Bottom Face
				Vector3 blb = currentPositionVectorBottom - radiusAdd - depthAdd;
				Vector3 brb = currentPositionVectorBottom + radiusAdd - depthAdd;
				Vector3 blf = currentPositionVectorBottom - radiusAdd + depthAdd;
				Vector3 brf = currentPositionVectorBottom + radiusAdd + depthAdd;
				
				vertices.AddRange(new Vector3[] {blb,brb,blf,brf});
				vertices.AddRange(new Vector3[] {blb,brb,blf,brf});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				//Right Face
				vertices.AddRange(new Vector3[] {trb,trf,brb,brf});
				vertices.AddRange(new Vector3[] {trb,trf,brb,brf});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				//Left Face
				vertices.AddRange(new Vector3[] {tlb,tlf,blb,blf});
				vertices.AddRange(new Vector3[] {tlb,tlf,blb,blf});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				//Front Face
				vertices.AddRange(new Vector3[] {tlf,trf,blf,brf});
				vertices.AddRange(new Vector3[] {tlf,trf,blf,brf});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				//Back Face
				vertices.AddRange(new Vector3[] {tlb,trb,blb,brb});
				vertices.AddRange(new Vector3[] {tlb,trb,blb,brb});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			}
			else if (this.ChartType == BarChartType.Pyramid) {
				// Top Face
				Vector3 top = currentPositionVector;

				int[] forwardQuad = {currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				int[] reverseQuad = {currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};

				// Bottom Face
				Vector3 blb = currentPositionVectorBottom - radiusAdd - depthAdd;
				Vector3 brb = currentPositionVectorBottom + radiusAdd - depthAdd;
				Vector3 blf = currentPositionVectorBottom - radiusAdd + depthAdd;
				Vector3 brf = currentPositionVectorBottom + radiusAdd + depthAdd;
				
				vertices.AddRange(new Vector3[] {blb,brb,blf,brf});
				vertices.AddRange(new Vector3[] {blb,brb,blf,brf});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				//Right Face
				vertices.AddRange(new Vector3[] {top,top,brb,brf});
				vertices.AddRange(new Vector3[] {top,top,brb,brf});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				//Left Face
				vertices.AddRange(new Vector3[] {top,top,blb,blf});
				vertices.AddRange(new Vector3[] {top,top,blb,blf});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				//Front Face
				vertices.AddRange(new Vector3[] {top,top,blf,brf});
				vertices.AddRange(new Vector3[] {top,top,blf,brf});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				
				//Back Face
				vertices.AddRange(new Vector3[] {top,top,blb,brb});
				vertices.AddRange(new Vector3[] {top,top,blb,brb});
				forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
				currentPoint += 4;
				reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
				currentPoint += 4;
				triangles.AddRange(forwardQuad);
				triangles.AddRange(reverseQuad);
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
			}
			else if (ChartType == BarChartType.Cylinder) {
				// You should only need the bottom, top, center, and radius to calculate these
				// As I do with the Pyramid type, you can apparently double up your vertices for tris and use quads

				for (int seg=0;seg<this.CylinderSegments;seg++) {
					int nextSeg = seg + 1;
					if (nextSeg == this.CylinderSegments) {
						nextSeg = 0;
					}
					float angle = (float)seg / (float)this.CylinderSegments * 360.0f;
					float nextAngle = (float)nextSeg / (float)this.CylinderSegments * 360.0f;

					Vector3 topCenter = currentPositionVector;
					Vector3 bottomCenter = currentPositionVectorBottom;

					Vector3 currentBottomPoint = this.PointAtRadiusAngle(angle,this.ElementRadius,bottomCenter);
					Vector3 nextBottomPoint = this.PointAtRadiusAngle(nextAngle,this.ElementRadius,bottomCenter);
					Vector3 currentTopPoint = this.PointAtRadiusAngle(angle,this.ElementRadius,topCenter);
					Vector3 nextTopPoint = this.PointAtRadiusAngle(nextAngle,this.ElementRadius,topCenter);

					//TOP FACE
					vertices.AddRange(new Vector3[] {topCenter,topCenter,currentTopPoint,nextTopPoint});
					vertices.AddRange(new Vector3[] {topCenter,topCenter,currentTopPoint,nextTopPoint});
					int[] forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
					currentPoint += 4;
					int[] reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
					currentPoint += 4;
					triangles.AddRange(forwardQuad);
					triangles.AddRange(reverseQuad);
					uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
					uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
					colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
					colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});

					//OUTWARD FACE
					vertices.AddRange(new Vector3[] {bottomCenter,bottomCenter,currentBottomPoint,nextBottomPoint});
					vertices.AddRange(new Vector3[] {bottomCenter,bottomCenter,currentBottomPoint,nextBottomPoint});
					forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
					currentPoint += 4;
					reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
					currentPoint += 4;
					triangles.AddRange(forwardQuad);
					triangles.AddRange(reverseQuad);
					uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
					uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
					colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
					colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});

					//BOTTOM FACE
					vertices.AddRange(new Vector3[] {currentTopPoint,nextTopPoint,currentBottomPoint,nextBottomPoint});
					vertices.AddRange(new Vector3[] {currentTopPoint,nextTopPoint,currentBottomPoint,nextBottomPoint});
					forwardQuad = new int[]{currentPoint + 2,currentPoint + 1,currentPoint + 0, currentPoint + 2,currentPoint + 3,currentPoint + 1};
					currentPoint += 4;
					reverseQuad = new int[]{currentPoint + 0,currentPoint + 1,currentPoint + 2, currentPoint + 1,currentPoint + 3,currentPoint + 2};
					currentPoint += 4;
					triangles.AddRange(forwardQuad);
					triangles.AddRange(reverseQuad);
					uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
					uvs.AddRange(new Vector2[] {new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1f,0f), new Vector2(1f,1f)});
					colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
					colors.AddRange(new Color[] {new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f)});
				}
			}
			
			xposition += halfRadius;
			xposition += SpaceBetweenDataElements;
		}

		runThreadDone = true;

	}

	Vector3 PointAtRadiusAngle(float angle, float radius, Vector3 center)
	{
		return new Vector3(center.x + (radius * Mathf.Cos(angle * Mathf.Deg2Rad)), center.y, center.z + (radius * Mathf.Sin(angle * Mathf.Deg2Rad)));
	}

	public void DrawChart() {
		this.runningChart = true;
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
		float halfDepth = this.MaxDepth / 2f;
		float halfWidth = this.SpaceBetweenDataElements / 2f;
		float halfRadius = ElementRadius / 2f;
		float xposition = 0f;

		for (int z=0;z<ChartData.ChartValues.Length;z++) {
			xposition += halfRadius;
			float currentHeightPercent = (ChartData.ChartValues[z].DataValue - minValue) / (maxValue - minValue);
			float currentHeightValue = Mathf.Lerp(0f,this.MaxHeight,currentHeightPercent);
			Vector3 currentPositionVector = new Vector3(xposition, currentHeightValue + this.BottomPadding, halfDepth);

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

			xposition += halfRadius;
			xposition += SpaceBetweenDataElements;
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
