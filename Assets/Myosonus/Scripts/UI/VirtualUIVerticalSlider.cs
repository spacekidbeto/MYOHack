using UnityEngine;
using System.Collections;

public class VirtualUIVerticalSlider : MonoBehaviour {

	public BoxCollider SliderAreaCollider;
	public SpriteRenderer SliderRenderer;
	public SpriteRenderer SlideKnobRenderer;
	Vector3 MinimumBounds;
	Vector3 MaximumBounds;

	public event VirtualUISliderValueChangedHandler SliderValueChanged;

	public float SizeY = 0f;
	public float HalfSizeY = 0f;
	public float CenterY = 0f;
	public float TopY = 0f;
	public float BottomY = 0f;

	float lastPercentage = 0f;
	Vector3 lastFingerPos = Vector3.zero;

	float PercentageValue = -1f;

	public bool Initialized = false;

	// Use this for initialization
	void Start () {
		this.MinimumBounds = SliderRenderer.bounds.min;
		this.MaximumBounds = SliderRenderer.bounds.max;

		this.SizeY = SliderAreaCollider.size.y;
		this.HalfSizeY = this.SizeY / 2f;
		this.CenterY = SliderAreaCollider.center.y;
		this.TopY = this.CenterY + this.HalfSizeY;
		this.BottomY = this.CenterY - this.HalfSizeY;

		this.Initialized = true;
	}

	public float GetPercentageValue() {
		return PercentageValue;
	}

	public void SetValue(float percentage) {
		float newY = ((this.SizeY) * percentage) + this.BottomY;
		Vector3 trans = SlideKnobRenderer.transform.localPosition;
		trans.y = newY;
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
			
			float totalDistance = (MaximumBounds.y - MinimumBounds.y);
			float percentage = 0f;

			if (hit.transform != this.transform) {
				percentage = lastPercentage;
				Vector3 maxScreen = Camera.main.WorldToScreenPoint(MaximumBounds);
				Vector3 minScreen = Camera.main.WorldToScreenPoint(MinimumBounds);
				float newDistance = maxScreen.y - minScreen.y;
				float newPercent = ((VirtualUIFunctions.FirstFingerCurrentPosition.y - minScreen.y) / newDistance);
				percentage = newPercent;
			}
			else
			{
				percentage = ((hitPointScreen.y - MinimumBounds.y) / totalDistance);
				lastPercentage = percentage;
				lastFingerPos = VirtualUIFunctions.FirstFingerCurrentPosition;
			}
			if (percentage > 0.999f) {
				percentage = 1f;
			}
			else if (percentage < 0f) {
				percentage = 0f;
			}

			float newY = ((this.SizeY) * percentage) + this.BottomY;
			Vector3 trans = SlideKnobRenderer.transform.localPosition;
			trans.y = newY;
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
