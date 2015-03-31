using UnityEngine;
using System.Collections;

public class VirtualUIColorPicker : MonoBehaviour {

	public VirtualUIVerticalSlider RedSlider;
	public VirtualUIVerticalSlider BlueSlider;
	public VirtualUIVerticalSlider GreenSlider;
	public VirtualUIVerticalSlider AlphaSlider;
	public TextMesh RGBText;
	public TextMesh HexText;
	public SpriteRenderer ColorBlock;
	public MeshRenderer ColorBlockMeshRenderer;

	public Color ColorValue = Color.white;

	bool redInit, blueInit, greenInit, alphaInit = false; 
	bool colorInit = false;

	// Use this for initialization
	void Awake () {
		this.RedSlider.SliderValueChanged += HandleRed;
		this.GreenSlider.SliderValueChanged += HandleGreen;
		this.BlueSlider.SliderValueChanged += HandleBlue;
		this.AlphaSlider.SliderValueChanged += HandleAlpha;
	}

	void CheckOtherSliders() {
		if (this.RedSlider.Initialized) this.redInit = true;
		if (this.BlueSlider.Initialized) this.blueInit = true;
		if (this.GreenSlider.Initialized) this.greenInit = true;
		if (this.AlphaSlider.Initialized) this.alphaInit = true;
	}
	
	void HandleAlpha (object sender, VirtualUISliderValueChangedArgs e)
	{
		this.ColorValue.a = e.Percentage;
		this.SetNewColor(this.ColorValue);
	}

	void HandleBlue (object sender, VirtualUISliderValueChangedArgs e)
	{
		this.ColorValue.b = e.Percentage;
		this.SetNewColor(this.ColorValue);
	}

	void HandleGreen (object sender, VirtualUISliderValueChangedArgs e)
	{
		this.ColorValue.g = e.Percentage;
		this.SetNewColor(this.ColorValue);
	}

	void HandleRed (object sender, VirtualUISliderValueChangedArgs e)
	{
		this.ColorValue.r = e.Percentage;
		this.SetNewColor(this.ColorValue);
	}

	public void SetNewColor(Color newValue) {
		this.RedSlider.SetValue(newValue.r);
		this.GreenSlider.SetValue(newValue.g);
		this.BlueSlider.SetValue(newValue.b);
		this.AlphaSlider.SetValue(newValue.a);
		RGBText.text = newValue.ToString().Replace("RGBA","");
		HexText.text = GetHexText(newValue);
		this.ColorValue = newValue;
		this.ColorBlock.color = newValue;
		this.ColorBlockMeshRenderer.material.color = newValue;
	}

	string GetHexText(Color colorValue) {
		int r = (int)(colorValue.r * 255f);
		int g = (int)(colorValue.g * 255f);
		int b = (int)(colorValue.b * 255f);
		int a = (int)(colorValue.a * 255f);
		return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + a.ToString("X2");
	}
	
	// Update is called once per frame
	void Update () {
		if (!this.colorInit) {
			this.CheckOtherSliders();
			if (this.redInit && this.blueInit && this.greenInit && this.alphaInit) {
				this.colorInit = true;
				this.SetNewColor(this.ColorValue);
			}
		}
	}
}
