using UnityEngine;
using System.Collections;

public class CameraBackgroundGradientScript : MonoBehaviour {

	public Color topColor = Color.blue;
	public Color bottomColor = Color.white;
	public Color middleColor = Color.white;
	public int gradientLayer = 7;

	float transitionStartTime = 1f;
	float currentTransitionTime = 0f;

	public Color transitionTopColor;
	public Color transitionMiddleColor;
	public Color transitionBottomColor;

	Color currentTopColor;
	Color currentMiddleColor;
	Color currentBottomColor;

	Material gradientMaterial;
	GameObject gradientPlane;
	Mesh gradientMesh;
	MeshFilter cameraMeshFilter;

	void Update() {
		if (currentTransitionTime > 0f) {
			currentTransitionTime -= Time.deltaTime;
			currentTopColor = VirtualUIFunctions.LerpColor(this.transitionTopColor, this.topColor, currentTransitionTime);
			currentMiddleColor = VirtualUIFunctions.LerpColor(this.transitionMiddleColor, this.middleColor, currentTransitionTime);
			currentBottomColor = VirtualUIFunctions.LerpColor(this.transitionBottomColor, this.bottomColor, currentTransitionTime);
			this.SetMesh();
		}
	}

	public void SetNewColors(GradientColorInformation newColors) {
		this.topColor = currentTopColor;
		this.middleColor = currentMiddleColor;
		this.bottomColor = currentBottomColor;
		this.transitionTopColor = newColors.TopColor;
		this.transitionMiddleColor = newColors.MiddleColor;
		this.transitionBottomColor = newColors.BottomColor;
		this.currentTransitionTime = transitionStartTime;
	}

	void SetMesh() {
		this.gradientMesh.colors = new Color[8] {
			currentTopColor,currentTopColor,
			currentMiddleColor,currentMiddleColor,
			currentMiddleColor,currentMiddleColor,
			currentBottomColor,currentBottomColor
		};
		this.cameraMeshFilter.mesh = this.gradientMesh;
	}

	void Awake () {	
		currentTopColor = topColor;
		currentMiddleColor = middleColor;
		currentBottomColor = bottomColor;

		gradientLayer = Mathf.Clamp(gradientLayer, 0, 31);
		if (!GetComponent<Camera>()) {
			Debug.LogError ("Must attach GradientBackground script to the camera");
			return;
		}
		
		GetComponent<Camera>().clearFlags = CameraClearFlags.Depth;
		GetComponent<Camera>().cullingMask = GetComponent<Camera>().cullingMask & ~(1 << gradientLayer);
		Camera gradientCam = new GameObject("Gradient Cam",typeof(Camera)).GetComponent<Camera>();
		gradientCam.depth = GetComponent<Camera>().depth-1;
		gradientCam.cullingMask = 1 << gradientLayer;
		
		this.gradientMaterial = new Material("Shader \"Vertex Color Only\"{Subshader{BindChannels{Bind \"vertex\", vertex Bind \"color\", color}Pass{}}}");
		this.gradientPlane = new GameObject("Gradient Plane", typeof(MeshFilter), typeof(MeshRenderer));
		this.gradientMesh = new Mesh();
		this.gradientMesh.vertices = new Vector3[8] {
			new Vector3(-100f,.577f,1f),
			new Vector3(100f,.577f,1f),
			new Vector3(-100f,0f,1f),
			new Vector3(100f,0f,1f),
			new Vector3(-100f,0f,1f),
			new Vector3(100f,0f,1f),
			new Vector3(-100f,-.577f,1f),
			new Vector3(100f,-.577f,1f)
		};
		this.gradientMesh.colors = new Color[8] {
			currentTopColor,currentTopColor,
			currentMiddleColor,currentMiddleColor,
			currentMiddleColor,currentMiddleColor,
			currentBottomColor,currentBottomColor
		};
		this.gradientMesh.triangles = new int[12] {
			0, 1, 2,
			1, 3, 2,
			
			4, 5, 6, 
			5, 7, 6
		};

		this.cameraMeshFilter = ((MeshFilter)this.gradientPlane.GetComponent(typeof(MeshFilter)));
		this.cameraMeshFilter.mesh = this.gradientMesh;
		this.gradientPlane.GetComponent<Renderer>().material = this.gradientMaterial;
		this.gradientPlane.layer = gradientLayer;
	}
}
