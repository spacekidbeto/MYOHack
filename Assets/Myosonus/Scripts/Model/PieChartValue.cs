using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

[ExecuteInEditMode]
public class PieChartValue : MonoBehaviour {

	public string DataName;
	public float DataValue;
	public float DataExtraHeight = 0f;
	public float DataExtraWidth = 0f;
	public int Segments = 64;

	public MeshFilter DataMeshFilter;
	public MeshRenderer DataMeshRenderer;
	public EaseOutProperties Easing;
	public VirtualUIFadableText Text;
	Mesh DataMesh = null;
	Mesh _oldMesh = null;
	float oldStartAngle = 0f;
	float oldEndAngle = 0f;
	float newStartAngle = 0f;
	float newEndAngle = 0f;

	float holeRadius = 0f;
	float radius = 0f;
	float explodeRadius = 0f;
	float height = 0f;
	Vector3 centerPoint = new Vector3(0f,0f,0f);

	List<Vector3> vertices = new List<Vector3>();
	List<Vector2> uvs = new List<Vector2>();
	List<int> triangles = new List<int>();
	List<Color> colors = new List<Color>();
	

	#if UNITY_WINRT || UNITY_WP8
	#else
	BackgroundWorker worker = new BackgroundWorker();
	#endif

	bool renderDone = false;
	int totalVerts = 0;

	Vector3 PointAtRadiusAngle(float angle, float radius, Vector3 center)
	{
		return new Vector3(center.x + (radius * Mathf.Cos(angle * Mathf.Deg2Rad)), center.y + (radius * Mathf.Sin(angle * Mathf.Deg2Rad)), center.z);
	}
	

	// Use this for initialization
	void Start () {
		this.DataMesh = new Mesh();
	}
	
	// Update is called once per frame
	void Update () {
		if (renderDone && this.vertices.Count == this.totalVerts) {
			renderDone = false;
			this.DataMesh.vertices = this.vertices.ToArray();
			this.DataMesh.triangles = this.triangles.ToArray();
			this.DataMesh.colors = this.colors.ToArray();
			this.DataMesh.uv = this.uvs.ToArray();
			this.DataMesh.RecalculateNormals();
			

			if (Application.isPlaying) {
				if (this.Easing != null) {
					try
					{
						this.Easing.SetDuration(1f);
					this.Easing.SetRenderer(this.DataMesh, this.DataMeshFilter, this._oldMesh);
					this.Easing.StartEasing();

					this._oldMesh = new Mesh();
					/*
					if (this._oldMesh == null) {
						this._oldMesh = new Mesh();
					}
					else {
						//this._oldMesh.Clear();
					}
					*/
					this._oldMesh.vertices = this.vertices.ToArray();
					this._oldMesh.triangles = this.triangles.ToArray();
					this._oldMesh.colors = this.colors.ToArray();
					this._oldMesh.uv = this.uvs.ToArray();
					this._oldMesh.RecalculateNormals();
					}
					catch(UnityException ex) {
						Debug.Log(ex.ToString());
					}
					//Debug.Log("New Mesh Set");
				}
				else {
					this.DataMeshFilter.sharedMesh = DataMesh;
				}
			}
			else {
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
#endif
				this.DataMeshFilter.sharedMesh = DataMesh;
			}

		}
	}

	public void RenderPie(float startAngle, float endAngle, float radius, float holeRadius, Vector3 center, float explodeRadius, float height) {
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		triangles = new List<int>();
		colors = new List<Color>();

		#if UNITY_WINRT || UNITY_WP8
		#else
		worker = new BackgroundWorker();
		worker.DoWork += HandleDoWork;
		#endif

		this.newStartAngle = startAngle;
		this.newEndAngle = endAngle;
		this.radius = radius;
		this.holeRadius = holeRadius;
		this.centerPoint = center;
		this.explodeRadius = explodeRadius;
		this.height = height;
		//this._oldMesh = this.DataMesh;
		//Debug.Log("Rendering " + this.transform.name);
		//this.DataMesh = new Mesh();

		#if UNITY_WINRT || UNITY_WP8
		this.HandleDoWork(this, null);
		#else
		worker.RunWorkerAsync();
		#endif
	}

	#if UNITY_WINRT || UNITY_WP8
	void HandleDoWork (object sender, System.EventArgs e) {
	#else
	void HandleDoWork (object sender, DoWorkEventArgs e) {
	#endif
	
		float middleAngle = ((float)(this.newEndAngle - this.newStartAngle) / 2f) + this.newStartAngle;
		float halfHeight = this.height / 2f;
		
		int vertex = 0;
		for (int z=0;z<this.Segments;z++) {
			float percentThroughPie = (float)z / (float)this.Segments;
			//float currentAngle = percentThroughPie / (float)(this.newEndAngle - this.newStartAngle) + this.newStartAngle;
			float currentAngle = percentThroughPie * (float)(this.newEndAngle - this.newStartAngle) + this.newStartAngle;

			float percentNextThroughPie = (float)(z+1) / (float)this.Segments;
			//float nextAngle = percentNextThroughPie / (float)(this.newEndAngle - this.newStartAngle) + this.newStartAngle;
			float nextAngle = percentNextThroughPie * (float)(this.newEndAngle - this.newStartAngle) + this.newStartAngle;

			Vector3 explodePoint = this.PointAtRadiusAngle(middleAngle, this.explodeRadius, this.centerPoint);

			Vector3 holePoint = this.PointAtRadiusAngle(currentAngle, this.holeRadius, this.centerPoint) + explodePoint;
			Vector3 radiusPoint = this.PointAtRadiusAngle(currentAngle, this.radius, this.centerPoint) + explodePoint;
			Vector3 holePointNext = this.PointAtRadiusAngle(nextAngle, this.holeRadius, this.centerPoint) + explodePoint;
			Vector3 radiusPointNext = this.PointAtRadiusAngle(nextAngle, this.radius, this.centerPoint) + explodePoint;

			Vector3 topHeight = new Vector3(0f,0f,halfHeight);
			Vector3 bottomHeight = new Vector3(0f,0f,-1f * halfHeight);

			Vector3 insideTopFirstPoint = holePoint + topHeight;
			Vector3 insideTopSecondPoint = holePointNext + topHeight;
			Vector3 outsideTopFirstPoint = radiusPoint + topHeight;
			Vector3 outsideTopSecondPoint = radiusPointNext + topHeight;
			Vector3 insideBottomFirstPoint = holePoint + bottomHeight;
			Vector3 insideBottomSecondPoint = holePointNext + bottomHeight;
			Vector3 outsideBottomFirstPoint = radiusPoint + bottomHeight;
			Vector3 outsideBottomSecondPoint = radiusPointNext + bottomHeight;

			if (z==0) {
				// Build the First Interior Face
				vertices.AddRange(new Vector3[]{insideTopFirstPoint,insideBottomFirstPoint,outsideTopFirstPoint,outsideBottomFirstPoint});
				triangles.AddRange(new int[]{vertex+0,vertex+1,vertex+2,vertex+1,vertex+3,vertex+2});
				vertex += 4;
				vertices.AddRange(new Vector3[]{insideTopFirstPoint,insideBottomFirstPoint,outsideTopFirstPoint,outsideBottomFirstPoint});
				triangles.AddRange(new int[]{vertex+2,vertex+1,vertex+0,vertex+2,vertex+3,vertex+1});
				vertex += 4;
				colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
				colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
			}

			//Build Inner Face
			vertices.AddRange(new Vector3[]{insideTopFirstPoint,insideTopSecondPoint,insideBottomFirstPoint,insideBottomSecondPoint});
			triangles.AddRange(new int[]{vertex+0,vertex+1,vertex+2,vertex+1,vertex+3,vertex+2});
			vertex += 4;
			vertices.AddRange(new Vector3[]{insideTopFirstPoint,insideTopSecondPoint,insideBottomFirstPoint,insideBottomSecondPoint});
			triangles.AddRange(new int[]{vertex+2,vertex+1,vertex+0,vertex+2,vertex+3,vertex+1});
			vertex += 4;
			colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
			colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
			uvs.AddRange(new Vector2[] {new Vector2(percentThroughPie,0f),new Vector2(percentNextThroughPie,0f),new Vector2(percentThroughPie,1f), new Vector2(percentNextThroughPie,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(percentThroughPie,0f),new Vector2(percentNextThroughPie,0f),new Vector2(percentThroughPie,1f), new Vector2(percentNextThroughPie,1f)});

			//Build Top Face
			vertices.AddRange(new Vector3[]{insideTopFirstPoint,insideTopSecondPoint,outsideTopFirstPoint,outsideTopSecondPoint});
			triangles.AddRange(new int[]{vertex+0,vertex+1,vertex+2,vertex+1,vertex+3,vertex+2});
			vertex += 4;
			vertices.AddRange(new Vector3[]{insideTopFirstPoint,insideTopSecondPoint,outsideTopFirstPoint,outsideTopSecondPoint});
			triangles.AddRange(new int[]{vertex+2,vertex+1,vertex+0,vertex+2,vertex+3,vertex+1});
			vertex += 4;
			colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
			colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
			uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
			uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});

			//Build Outer Face
			vertices.AddRange(new Vector3[]{outsideTopFirstPoint,outsideTopSecondPoint,outsideBottomFirstPoint,outsideBottomSecondPoint});
			triangles.AddRange(new int[]{vertex+0,vertex+1,vertex+2,vertex+1,vertex+3,vertex+2});
			vertex += 4;
			vertices.AddRange(new Vector3[]{outsideTopFirstPoint,outsideTopSecondPoint,outsideBottomFirstPoint,outsideBottomSecondPoint});
			triangles.AddRange(new int[]{vertex+2,vertex+1,vertex+0,vertex+2,vertex+3,vertex+1});
			vertex += 4;
			colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
			colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
			uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
			uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});

			//Build Bottom Face
			vertices.AddRange(new Vector3[]{outsideBottomFirstPoint,outsideBottomSecondPoint,insideBottomFirstPoint,insideBottomSecondPoint});
			triangles.AddRange(new int[]{vertex+0,vertex+1,vertex+2,vertex+1,vertex+3,vertex+2});
			vertex += 4;
			vertices.AddRange(new Vector3[]{outsideBottomFirstPoint,outsideBottomSecondPoint,insideBottomFirstPoint,insideBottomSecondPoint});
			triangles.AddRange(new int[]{vertex+2,vertex+1,vertex+0,vertex+2,vertex+3,vertex+1});
			vertex += 4;
			colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
			colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
			uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
			uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});

			if (z==this.Segments-1) {
				// Build the Last Interior Face
				vertices.AddRange(new Vector3[]{insideTopSecondPoint,insideBottomSecondPoint,outsideTopSecondPoint,outsideBottomSecondPoint});
				triangles.AddRange(new int[]{vertex+0,vertex+1,vertex+2,vertex+1,vertex+3,vertex+2});
				vertex += 4;
				vertices.AddRange(new Vector3[]{insideTopSecondPoint,insideBottomSecondPoint,outsideTopSecondPoint,outsideBottomSecondPoint});
				triangles.AddRange(new int[]{vertex+2,vertex+1,vertex+0,vertex+2,vertex+3,vertex+1});
				vertex += 4;
				colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
				colors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
				uvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
			}
		}

		this.oldStartAngle = this.newStartAngle;
		this.oldEndAngle = this.newEndAngle;
		this.totalVerts = vertex;
		renderDone = true;
	}
}
