using UnityEngine;
using System.Collections;

public class VirtualUIModalDropdown : MonoBehaviour {

	public VirtualUIButton CancelButton;
	public TextMesh CancelText;
	public VirtualUIVerticalSlider VerticalBar;
	public VirtualUIButton ItemText;
	public Color ItemInactiveColor = Color.white;
	public Color ItemActiveColor = Color.cyan;
	public Color ItemInvisibleColor = new Color(1f,1f,1f,0f);
	public float SpaceBetweenRows = 0.1f;
	public VirtualUIRepeater.RowDirection RowDirection = VirtualUIRepeater.RowDirection.YMinus;
	public RepeaterRowCollection DataValues;
	public int SelectedIndex = 0;
	public string SelectedValue;
	public event System.EventHandler SelectedIndexChanged;
	public string DisplayMember;
	public string ValueMember;
	public float EndValue = 0f;
	public Transform ItemHolder;
	Vector3 StartingPosition = Vector3.zero;
	VirtualUIButton [] Texts;
	public BoxCollider UpperBoundary;
	public BoxCollider LowerBoundary;
	bool isOpen = false;

	// Use this for initialization
	void Start () {
		this.ItemText.TextObject.GetComponent<Renderer>().sortingLayerID = 1;
		this.CancelText.GetComponent<Renderer>().sortingLayerID = 1;

		this.CancelButton.ClickUp += HandleCancel;
		this.VerticalBar.SliderValueChanged += HandleSliderValueChanged;
		this.ItemText.TextObject.color = this.ItemInvisibleColor;
		this.StartingPosition = this.ItemHolder.transform.localPosition;
	}

	void RemoveOldItems() {
		for (int z= this.ItemHolder.transform.childCount - 1;z >= 0;z--)
		{
			Transform child = this.ItemHolder.transform.GetChild(z);
			if (child.name.StartsWith("Item_")) {
				Destroy (child);
			}
		}
	}

	public void BuildList() {
		this.RemoveOldItems();
		this.ItemHolder.transform.localPosition = this.StartingPosition;
		Vector3 rowPosition = this.StartingPosition;
		this.Texts = new VirtualUIButton[this.DataValues.Rows.Length];
		for (int z=0;z<this.DataValues.Rows.Length;z++) {
			string displayValue = this.DataValues.Rows[z][DisplayMember].PropertyValue;
			string propValue = this.DataValues.Rows[z][ValueMember].PropertyValue;

			VirtualUIButton newMesh = (VirtualUIButton)Instantiate(this.ItemText, rowPosition, this.ItemText.transform.rotation);
			newMesh.ClickUp += HandleDropdownSelect;
			newMesh.TextObject.color = ItemInactiveColor;
			newMesh.TextObject.text = displayValue;
			newMesh.SelectionIndex = z;
			newMesh.DisplayText = displayValue;
			newMesh.ValueText = propValue;
			newMesh.transform.name = "Item_" + z;
			newMesh.transform.parent = this.ItemHolder.transform;
			newMesh.transform.localPosition = rowPosition;
			rowPosition.y -= SpaceBetweenRows;
			this.Texts[z] = newMesh;
		}
		this.EndValue = rowPosition.y;
		this.SetItemVisibility();
	}

	void HandleDropdownSelect (object sender, System.EventArgs e)
	{
		VirtualUIButton obj = (VirtualUIButton)sender;
		SelectedValue = obj.ValueText;
		SelectedIndex = obj.SelectionIndex;
		if (this.SelectedIndexChanged != null) {
			this.SelectedIndexChanged(this, new System.EventArgs());
		}
	}

	void SetItemVisibility() {
		float TopY = this.VerticalBar.TopY;
		float BottomY = this.VerticalBar.BottomY;
		for (int z= this.ItemHolder.transform.childCount - 1;z >= 0;z--)
		{
			Transform child = this.ItemHolder.transform.GetChild(z);
			if (child.name.StartsWith("Item_")) {
				Color newColor = Color.white;
				//Debug.Log(child.transform.localPosition + " / " + TopY + " / " + BottomY);
				VirtualUIButton mesh = Texts[z];

				if (mesh.GetComponent<Renderer>().bounds.min.y > UpperBoundary.bounds.min.y) {
					if (mesh.GetComponent<Renderer>().bounds.min.y < UpperBoundary.bounds.max.y) {
						float distance = UpperBoundary.bounds.max.y - UpperBoundary.bounds.min.y;
						float value = mesh.GetComponent<Renderer>().bounds.min.y - UpperBoundary.bounds.min.y;
						float percent = value / distance;
						newColor = Color.Lerp(this.ItemInactiveColor,this.ItemInvisibleColor,percent);
					}
					else
					{
						newColor = this.ItemInvisibleColor;
					}
				}
				else if (mesh.GetComponent<Renderer>().bounds.max.y < LowerBoundary.bounds.max.y) {
					if (mesh.GetComponent<Renderer>().bounds.max.y > LowerBoundary.bounds.min.y) {
						float distance = LowerBoundary.bounds.max.y - LowerBoundary.bounds.min.y;
						float value = LowerBoundary.bounds.max.y - mesh.GetComponent<Renderer>().bounds.max.y;
						float percent = value / distance;
						newColor = Color.Lerp(this.ItemInactiveColor,this.ItemInvisibleColor,percent);
					}
					else
					{
						newColor = this.ItemInvisibleColor;
					}
				}
				else {
					newColor = this.ItemInactiveColor;
				}

				Texts[z].TextObject.color = newColor;
			}
		}
	}

	void HandleSliderValueChanged (object sender, VirtualUISliderValueChangedArgs e)
	{

		float StartValue = this.StartingPosition.y;
		float CurrentValue = StartValue - ((1f - e.Percentage) * (EndValue - StartValue));
		Vector3 vector = this.ItemHolder.transform.localPosition;
		vector.y = CurrentValue;
		this.ItemHolder.transform.localPosition = vector;
		this.SetItemVisibility();
		//Debug.Log(e.Percentage);
	}

	void HandleCancel (object sender, System.EventArgs e)
	{
		isOpen = false;
		VirtualUIFunctions.IsModal = false;
		this.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	}
}
