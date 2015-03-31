using UnityEngine;
using System.Collections;

// Note: jernest - 11/15/2014
// Technically you can just use a Vertical Slider, but I made this for simplicity's sake.
public class VirtualUIHorizontalSlider : MonoBehaviour {

	public BoxCollider SliderAreaCollider;
	public SpriteRenderer SliderRenderer;
	public SpriteRenderer SlideKnobRenderer;
	Vector3 MinimumBounds;
	Vector3 MaximumBounds;
	
	public event VirtualUISliderValueChangedHandler SliderValueChanged;
	
	public float SizeX = 0f;
	public float HalfSizeX = 0f;
	public float CenterX = 0f;
	public float TopX = 0f;
	public float BottomX = 0f;
	
	float lastPercentage = 0f;
	Vector3 lastFingerPos = Vector3.zero;
	
	float PercentageValue = -1f;
	
	public bool Initialized = false;
	
	// Use this for initialization
	void Start () {
		this.MinimumBounds = SliderRenderer.bounds.min;
		this.MaximumBounds = SliderRenderer.bounds.max;
		
		this.SizeX = SliderAreaCollider.size.x;
		this.HalfSizeX = this.SizeX / 2f;
		this.CenterX = SliderAreaCollider.center.x;
		this.TopX = this.CenterX + this.HalfSizeX;
		this.BottomX = this.CenterX - this.HalfSizeX;
		
		this.Initialized = true;
	}
	
	public float GetPercentageValue() {
		return PercentageValue;
	}
	
	public void SetValue(float percentage) {
		float newY = ((this.SizeX) * percentage) + this.BottomX;
		Vector3 trans = SlideKnobRenderer.transform.localPosition;
		trans.x = newY;
		SlideKnobRenderer.transform.localPosition = trans;
		
		if (this.PercentageValue != percentage)
		{
			this.PercentageValue = percentage;
			
			if (this.SliderValueChanged != null) {
				this.SliderValueChanged(this, new VirtualUISliderValueChangedArgs(percentage));
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void FixedUpdate() {
		if (VirtualUIFunctions.IsInTouchState) {
			if (VirtualUIFunctions.FirstFingerObjectTag != "Slider") return;
			if (VirtualUIFunctions.FirstFingerObjectHit != this.transform) return;
			
			RaycastHit hit;
			Ray ray, rayStart;
			ray = Camera.main.ScreenPointToRay(VirtualUIFunctions.FirstFingerCurrentPosition);
			Physics.Raycast(ray, out hit);
			Vector3 hitPointScreen = hit.point;
			
			float totalDistance = (MaximumBounds.x - MinimumBounds.x);
			float percentage = 0f;
			
			if (hit.transform != this.transform) {
				percentage = lastPercentage;
				Vector3 maxScreen = Camera.main.WorldToScreenPoint(MaximumBounds);
				Vector3 minScreen = Camera.main.WorldToScreenPoint(MinimumBounds);
				float newDistance = maxScreen.x - minScreen.x;
				float newPercent = ((VirtualUIFunctions.FirstFingerCurrentPosition.x - minScreen.x) / newDistance);
				percentage = newPercent;
			}
			else
			{
				percentage = ((hitPointScreen.x - MinimumBounds.x) / totalDistance);
				lastPercentage = percentage;
				lastFingerPos = VirtualUIFunctions.FirstFingerCurrentPosition;
			}
			if (percentage > 0.999f) {
				percentage = 1f;
			}
			else if (percentage < 0f) {
				percentage = 0f;
			}
			
			float newY = ((this.SizeX) * percentage) + this.BottomX;
			Vector3 trans = SlideKnobRenderer.transform.localPosition;
			trans.x = newY;
			SlideKnobRenderer.transform.localPosition = trans;
			
			if (this.PercentageValue != percentage)
			{
				this.PercentageValue = percentage;
				
				if (this.SliderValueChanged != null) {
					this.SliderValueChanged(this, new VirtualUISliderValueChangedArgs(percentage));
				}
			}
			
		}
	}
	
	void LateUpdate() {
	}
}
