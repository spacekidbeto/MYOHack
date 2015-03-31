using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EaseOutProperties : MonoBehaviour {

	float _duration = 1f;
	float _currentTime = 0f;
	bool _started = false;
	public bool UseColor = false;
	public bool UsePosition = false;
	public bool UseScale = false;
	public bool UseRotation = false;
	public bool UseEaseIn = false;

	Color _startColor;
	Color _endColor;
	Vector3 _startPosition;
	Vector3 _endPosition;
	Vector3 _startScale;
	Vector3 _endScale;
	Vector3 _startRotation;
	Vector3 _endRotation;

	Vector3[] _startLineRendererPositions;
	Vector3[] _endLineRendererPositions;

	SpriteRenderer _SpriteRendererObject;
	TextMesh _TextObject;
	Mesh _MeshObject;
	Mesh _OldMeshObject;
	Mesh _EndMeshObject;
	int _SubMeshCount = 1;
	List<int[]> _SubMeshTriangles = new List<int[]>();
	

	Vector3[] _oldMeshVertices;
	Vector3[] _newMeshVertices;
	MeshFilter _MeshFilterObject;
	LineRenderer _LineRendererObject;
	Transform _TransformObject;

	bool _useSprite = false;
	bool _useText = false;
	bool _useMesh = false;
	bool _useLine = false;
	bool _useTransform = false;

	public bool UseSprite {	get { return _useSprite;} set {_useSprite = value; }}
	public bool UseText {	get { return _useText;} set {_useText = value; }}
	public bool UseMesh {	get { return _useMesh;} set {_useMesh = value; }}
	public bool UseLine {	get { return _useLine;} set {_useLine = value; }}
	public bool UseTransform {	get { return _useTransform;} set {_useTransform = value; }}

	
	bool _positiveDirectionRotation = false;

	public event System.EventHandler Completed;
	public event System.EventHandler Started;

	public void ResetUsage() {
		this._started = false;
		this.UseColor = false;
		this.UsePosition = false;
		this.UseScale = false;
		this.UseRotation = false;
		this._useLine = false;
		this._useMesh = false;
		this._useSprite = false;
		this._useText = false;
		this._useTransform = false;
	}

	public void SetRenderer(Transform transformObect) {
		this._useTransform = true;
		this._TransformObject = transformObect;
	}
	
	public void SetRenderer(SpriteRenderer rendererObject) {
		this._useSprite = true;
		this._SpriteRendererObject = rendererObject;
	}

	public void SetRenderer(TextMesh textObject)
	{
		this._useText = true;
		this._TextObject = textObject;
	}

	public void SetRenderer(LineRenderer lineRender, Vector3[] oldPositions, Vector3[] newPositions) {
		this._useLine = true;
		this._LineRendererObject = lineRender;

		if (oldPositions == null) {
			oldPositions = new Vector3[newPositions.Length];
			for (int z=0;z<oldPositions.Length;z++) {
				oldPositions[z] = new Vector3(0f,0f,0f);
			}
		}
		this._startLineRendererPositions = oldPositions;
		this._endLineRendererPositions = newPositions;
		lineRender.SetVertexCount(newPositions.Length);
	}

	public void SetRenderer(Mesh meshObject, MeshFilter filter, Mesh oldMeshObject) {
		try 
		{
			this._useMesh = true;
			this._MeshObject = meshObject;
			this._MeshFilterObject = filter;
			this._OldMeshObject = oldMeshObject;
			if (this._OldMeshObject == null) {
				this._OldMeshObject = new Mesh();
				this._OldMeshObject.vertices = new Vector3[this._MeshObject.vertexCount];
				for (int z=0;z<this._OldMeshObject.vertexCount;z++) {
					this._OldMeshObject.vertices[z] = new Vector3(0f,0f,0f);
				}
				this._OldMeshObject.colors = this._MeshObject.colors;
				this._OldMeshObject.triangles = this._MeshObject.triangles;
			}
			else if (this._MeshObject.vertices.Length != this._OldMeshObject.vertices.Length) {
				List<Vector3> newVerts = new List<Vector3>();
				List<int> newTris = new List<int>();
				List<Color> newColors = new List<Color>();
				List<Vector2> newUvs = new List<Vector2>();
				int iterations = 0;
				int vertex = newVerts.Count;
				bool newMeshBigger = false;
				if (this._MeshObject.vertices.Length > this._OldMeshObject.vertices.Length) {
					newMeshBigger = true;
					iterations = (this._MeshObject.vertices.Length - this._OldMeshObject.vertices.Length) / 8;
					newVerts.AddRange(this._OldMeshObject.vertices);
					newTris.AddRange(this._OldMeshObject.triangles);
					newColors.AddRange(this._OldMeshObject.colors);
					newUvs.AddRange(this._OldMeshObject.uv);
				}
				else {
					iterations = (this._OldMeshObject.vertices.Length - this._MeshObject.vertices.Length) / 8;
					newVerts.AddRange(this._OldMeshObject.vertices);
					newTris.AddRange(this._OldMeshObject.triangles);
					newColors.AddRange(this._OldMeshObject.colors);
					newUvs.AddRange(this._OldMeshObject.uv);
				}
				for (int z=0;z<iterations;z++) {
					newVerts.AddRange(new Vector3[] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero});
					newTris.AddRange(new int[] { vertex + 0, vertex + 1, vertex + 2, vertex + 1, vertex + 3, vertex + 2 });
					vertex += 4;
					newVerts.AddRange(new Vector3[] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero});
					newTris.AddRange(new int[] { vertex + 2, vertex + 1, vertex + 0, vertex + 2, vertex + 3, vertex + 1 });
					vertex += 4;
					newColors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
					newColors.AddRange(new Color[]{Color.white,Color.white,Color.white,Color.white});
					newUvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
					newUvs.AddRange(new Vector2[] {new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)});
				}
				if (newMeshBigger) {
					this._OldMeshObject = new Mesh();
					this._OldMeshObject.vertices = newVerts.ToArray();
					this._OldMeshObject.triangles = newTris.ToArray();
					this._OldMeshObject.colors = newColors.ToArray();
					this._OldMeshObject.uv = newUvs.ToArray();
				}
				else {
					this._MeshObject = new Mesh();
					this._MeshObject.vertices = newVerts.ToArray();
					this._MeshObject.triangles = newTris.ToArray();
					this._MeshObject.colors = newColors.ToArray();
					this._MeshObject.uv = newUvs.ToArray();
				}
			}
			/*
			if (this._MeshObject.subMeshCount > 1) {
				this._SubMeshCount = this._MeshObject.subMeshCount;
				this._SubMeshTriangles= new List<int[]>();
				for(int z=0;z<this._SubMeshCount;z++) {
					int[] triangles = this._MeshObject.GetTriangles(z);
					this._SubMeshTriangles.Add(triangles);
				}
			}
			*/
			this._EndMeshObject = this._MeshObject;

			this._oldMeshVertices = this._OldMeshObject.vertices;
			this._newMeshVertices = this._EndMeshObject.vertices;
		}
		catch
		{
			Debug.Log("Mesh Load Failed");
		}
	}

	public void SetEaseOutColor(Color endColor) {
		this.UseColor = true;
		if (this._useSprite) this._startColor = this._SpriteRendererObject.color;
		if (this._useText) this._startColor = this._TextObject.color;
		this._endColor = endColor;
	}

	public void SetPosition(Vector3 endPosition) {
		this.UsePosition = true;
		if (this._useSprite) this._startPosition = this._SpriteRendererObject.transform.localPosition;
		if (this._useText) this._startPosition = this._TextObject.transform.localPosition;
		if (this._useLine) this._startPosition = this._LineRendererObject.transform.localPosition;
		if (this._useTransform) this._startPosition = this._TransformObject.transform.localPosition;
		this._endPosition = endPosition;
	}

	public void SetRotation(Vector3 endRotation, bool positiveDirection) {
		this.UseRotation = true;
		Vector3 startRot = new Vector3(0f,0f,0f);
		if (this._useSprite) startRot = this._SpriteRendererObject.transform.localEulerAngles;
		if (this._useText) startRot = this._TextObject.transform.localEulerAngles;
		if (this._useLine) startRot = this._LineRendererObject.transform.localEulerAngles;
		if (this._useTransform) startRot = this._TransformObject.transform.localEulerAngles;
		if (startRot.x > 180f) {
			startRot.x = 360f - startRot.x;
		}
		if (startRot.y > 180f) {
			startRot.y = 360f - startRot.y;
		}
		if (startRot.z > 180f) {
			startRot.z = 360f - startRot.z;
		}
		this._startRotation = startRot;
		this._endRotation = endRotation;
	}

	public void SetScale(Vector3 endScale) {
		this.UseScale = true;
		if (this._useSprite) this._startScale = this._SpriteRendererObject.transform.localScale;
		if (this._useLine) this._startScale = this._LineRendererObject.transform.localScale;
		if (this._useText) this._startScale = this._TextObject.transform.localScale;
		if (this._useTransform) this._startScale = this._TransformObject.transform.localScale;
		this._endScale = endScale;
	}

	public void SetDuration(float duration) {
		this._duration = duration;
	}

	public void StartEasing() {
		this._currentTime = 0f;
		this._started = true;
		if (this.Started != null) {
			this.Started(this, new System.EventArgs());
		}
	}

	public void StopEasing() {
		this.ResetUsage();
	}

	float easeOutCubic (float currentTime, float startValue, float endValue, float duration) {
		currentTime /= duration;
		currentTime--;
		float deltaValue = endValue - startValue;
		return deltaValue*(currentTime*currentTime*currentTime + 1f) + startValue;
	}

	float easeOutCubicRotation (float currentTime, float startValue, float endValue, float duration) {
		currentTime /= duration;
		float factor = 0f;
		if (!UseEaseIn) {
			currentTime--;
			factor = 1f;
		}

		if (_positiveDirectionRotation) {
			float deltaValue = endValue - startValue;
			return deltaValue*(currentTime*currentTime*currentTime + factor) + startValue;
		}
		else {
			float deltaValue = startValue - endValue;
			return startValue - deltaValue*(currentTime*currentTime*currentTime + factor) ;
		}
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (this._started) {
			if (this._currentTime >= this._duration) {
				this.ResetUsage();
				if (this._OldMeshObject != null) {
					this._OldMeshObject.Clear();
				}
				if (this.Completed != null) {
					this.Completed(this, new System.EventArgs());
				}
			}
			else {
				this._currentTime += Time.deltaTime;

				if (this.UseColor) {
					float r = this.easeOutCubic(this._currentTime, this._startColor.r, this._endColor.r, this._duration);
					float g = this.easeOutCubic(this._currentTime, this._startColor.g, this._endColor.g, this._duration);
					float b = this.easeOutCubic(this._currentTime, this._startColor.b, this._endColor.b, this._duration);
					float a = this.easeOutCubic(this._currentTime, this._startColor.a, this._endColor.a, this._duration);
					if (this._useSprite) this._SpriteRendererObject.color = new Color(r,g,b,a);
					if (this._useText) this._TextObject.color = new Color(r,g,b,a);
				}
				if (this.UsePosition) {
					float x = this.easeOutCubic(this._currentTime, this._startPosition.x, this._endPosition.x, this._duration);
					float y = this.easeOutCubic(this._currentTime, this._startPosition.y, this._endPosition.y, this._duration);
					float z = this.easeOutCubic(this._currentTime, this._startPosition.z, this._endPosition.z, this._duration);
					if (this._useSprite) this._SpriteRendererObject.transform.localPosition = new Vector3(x,y,z);
					if (this._useText) this._TextObject.transform.localPosition = new Vector3(x,y,z);
					if (this._useLine) this._LineRendererObject.transform.localPosition = new Vector3(x,y,z);
					if (this._useTransform) this._TransformObject.transform.localPosition = new Vector3(x,y,z);
				}
				if (this.UseRotation) {
					float x = this.easeOutCubicRotation(this._currentTime, this._startRotation.x, this._endRotation.x, this._duration);
					float y = this.easeOutCubicRotation(this._currentTime, this._startRotation.y, this._endRotation.y, this._duration);
					float z = this.easeOutCubicRotation(this._currentTime, this._startRotation.z, this._endRotation.z, this._duration);
					if (this._useSprite) this._SpriteRendererObject.transform.localEulerAngles = new Vector3(x,y,z);
					if (this._useText) this._TextObject.transform.localEulerAngles = new Vector3(x,y,z);
					if (this._useLine) this._LineRendererObject.transform.localEulerAngles = new Vector3(x,y,z);
					if (this._useTransform) this._TransformObject.transform.localEulerAngles = new Vector3(x,y,z);
				}
				if (this.UseScale) {
					float x = this.easeOutCubic(this._currentTime, this._startScale.x, this._endScale.x, this._duration);
					float y = this.easeOutCubic(this._currentTime, this._startScale.y, this._endScale.y, this._duration);
					float z = this.easeOutCubic(this._currentTime, this._startScale.z, this._endScale.z, this._duration);
					if (this._useSprite) this._SpriteRendererObject.transform.localScale = new Vector3(x,y,z);
					if (this._useText) this._TextObject.transform.localScale = new Vector3(x,y,z);
					if (this._useLine) this._LineRendererObject.transform.localScale = new Vector3(x,y,z);
					if (this._useTransform) this._TransformObject.transform.localScale = new Vector3(x,y,z);
				}
				if (this.UseLine) {
					for (int w=0;w<this._endLineRendererPositions.Length;w++) {
						float x = this.easeOutCubic(this._currentTime, this._startLineRendererPositions[w].x, this._endLineRendererPositions[w].x, this._duration);
						float y = this.easeOutCubic(this._currentTime, this._startLineRendererPositions[w].y, this._endLineRendererPositions[w].y, this._duration);
						float z = this.easeOutCubic(this._currentTime, this._startLineRendererPositions[w].z, this._endLineRendererPositions[w].z, this._duration);
						this._LineRendererObject.SetPosition(w, new Vector3(x,y,z));
					}
				}
			}
		}
	}

	void LateUpdate() {
		if (this._started && this._currentTime < this._duration) {

			try {

			if (this.UseMesh) {
				Vector3[] positions = new Vector3[this._MeshObject.vertexCount];
				for (int w=0;w<positions.Length;w++) {
					float x = this.easeOutCubic(this._currentTime, this._oldMeshVertices[w].x, this._newMeshVertices[w].x, this._duration);
					float y = this.easeOutCubic(this._currentTime, this._oldMeshVertices[w].y, this._newMeshVertices[w].y, this._duration);
					float z = this.easeOutCubic(this._currentTime, this._oldMeshVertices[w].z, this._newMeshVertices[w].z, this._duration);
					positions[w] = new Vector3(x,y,z);
				}
				//Mesh newMesh = new Mesh();
				Mesh newMesh = this._MeshFilterObject.sharedMesh;
				newMesh.vertices = positions;

				newMesh.colors = this._MeshObject.colors;
				if (this._SubMeshCount > 1) {
					newMesh.subMeshCount = this._SubMeshCount;
					for(int z=0;z<this._SubMeshCount;z++) {
						newMesh.SetTriangles(this._SubMeshTriangles[z],z);
					}
				}
				else {
					newMesh.triangles = this._MeshObject.triangles;
				}
				newMesh.uv = this._MeshObject.uv;
				newMesh.normals = this._MeshObject.normals;

				//Mesh currentMesh = this._MeshFilterObject.sharedMesh;
				this._MeshFilterObject.sharedMesh = newMesh;
				//currentMesh.Clear();
			}
			}
			catch {

			}
		}
	}
}
