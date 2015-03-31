using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VirtualUIComboBox : MonoBehaviour {

	public RepeaterRowCollection DataValues;
	public int SelectedIndex = 0;
	public event System.EventHandler SelectedIndexChanged;
	public VirtualUIButton LeftButton;
	public VirtualUIButton RightButton;
	public VirtualUIButton BoxButton;
	public TextMesh ItemText;
	public string DisplayMember;
	public string ValueMember;
	public VirtualUIVerticalSlider Slider;
	public VirtualUIButton ComboItemTemplate;
	public Transform ComboItemHolder;
	public VirtualUIButton [] ComboItems;
	public float SpaceBetweenRows = 2f;
	public Color ItemInactiveColor = Color.white;
	public Color ItemActiveColor = Color.cyan;
	public Color ItemInvisibleColor = new Color(1f,1f,1f,0f);
	public BoxCollider UpperBoundary;
	public BoxCollider LowerBoundary;


	Vector3 StartingPosition = Vector3.zero;
	float EndValue = 0f;

	bool PulloutActive = true;
	public GameObject PulloutObject;

	public VirtualUIDataLoader Loader;
	public bool StartLoaderAutomatically = true;

	// Use this for initialization
	void Start () {
		LeftButton.ClickUp += HandleClickUpLeft;
		RightButton.ClickUp += HandleClickUpRight;
		BoxButton.ClickUp += HandleBoxClick;
		Slider.SliderValueChanged += HandleSliderValueChanged;
		this.ComboItemTemplate.TextObject.color = this.ItemInvisibleColor;

		if (Loader != null) {
			Loader.RowsLoaded += HandleRowsLoaded;
		}
		if (StartLoaderAutomatically) {
			this.StartLoader();
		}
	}

	public void StartLoader() {
		if (Loader != null) {
			Loader.JSONDownloader.StartDownload();
		}
	}
	
	void HandleRowsLoaded (object sender, System.EventArgs e)
	{
		if (this.DataValues != null)
		{
			this.KillPreviousItems();
			Destroy(this.DataValues);
		}
		this.DataValues = (RepeaterRowCollection)this.gameObject.AddComponent(typeof(RepeaterRowCollection));
		this.DataValues.Rows = this.Loader.Rows.ToArray();
		this.BuildList();
		TriggerSelectionChanged();
		this.SetItemText();
	}

	void KillPreviousItems() {
		int childCount = ComboItemHolder.childCount;
		for (int z = childCount - 1;z >= 0;z--) {
			Transform child = ComboItemHolder.GetChild(z);
			Destroy (child);
		}
	}

	public bool GetPulloutActive() {
		return this.PulloutActive;
	}

	public void SetPulloutActive(bool active) {
		this.PulloutActive = active;
		this.PulloutObject.SetActive(this.PulloutActive);
	}

	public void TogglePulloutActive() {
		this.PulloutActive = !this.PulloutActive;
		this.PulloutObject.SetActive(this.PulloutActive);
	}

	public void BuildList() {
		bool isPulloutActive = this.GetPulloutActive();
		if (!isPulloutActive) {
			this.SetPulloutActive(true);
		}
		KillPreviousItems();
		Vector3 position = this.ComboItemHolder.transform.localPosition;
		this.StartingPosition = position;
		ComboItems = new VirtualUIButton[DataValues.Rows.Length];
		for(int z=0;z<DataValues.Rows.Length;z++) {
			string text = DataValues.Rows[z][DisplayMember].PropertyValue;
			string value = DataValues.Rows[z][ValueMember].PropertyValue;
			VirtualUIButton button = (VirtualUIButton)Instantiate(this.ComboItemTemplate, Vector3.zero, this.ComboItemTemplate.transform.rotation);
			button.transform.parent = this.ComboItemHolder.transform;
			button.transform.name = "ComboItem_" + z;
			button.transform.localPosition = position;
			button.transform.localScale = this.ComboItemTemplate.transform.localScale;
			button.DisplayText = text;
			button.ValueText = value;
			button.SelectionIndex = z;
			button.TextObject.color = ItemInactiveColor;
			button.TextObject.text = text;
			button.ClickUp += HandleItemClickUp;
			ComboItems[z] = button;
			position.y -= SpaceBetweenRows;
		}
		this.EndValue = position.y;
		this.SetItemVisibility();
		this.SetPulloutActive(isPulloutActive);
	}

	void HandleItemClickUp (object sender, System.EventArgs e)
	{
		VirtualUIButton obj = (VirtualUIButton)sender;
		SelectedIndex = obj.SelectionIndex;
		this.SetItemText();
		TriggerSelectionChanged();
		this.SetItemVisibility();
	}

	void HandleSliderValueChanged (object sender, VirtualUISliderValueChangedArgs e)
	{
		float StartValue = this.StartingPosition.y;
		//Debug.Log((1f - e.Percentage) + " / " + EndValue + " / " + StartValue);
		float CurrentValue = StartValue - ((1f - e.Percentage) * (EndValue - StartValue));
		Vector3 vector = this.ComboItemHolder.transform.localPosition;
		vector.y = CurrentValue;
		this.ComboItemHolder.transform.localPosition = vector;
		this.SetItemVisibility();
	}

	void SetItemVisibility() {
		bool lastPulloutActive = this.PulloutActive;
		if (!this.PulloutActive) TogglePulloutActive();
		float TopY = this.Slider.TopY;
		float BottomY = this.Slider.BottomY;
		for (int z= this.ComboItemHolder.transform.childCount - 1;z >= 0;z--)
		{
			Transform child = this.ComboItemHolder.transform.GetChild(z);
			if (child.name.StartsWith("ComboItem_")) {
				Color newColor = Color.white;
				//Debug.Log(child.transform.localPosition + " / " + TopY + " / " + BottomY);
				VirtualUIButton mesh = ComboItems[z];

				bool isItemTouchable = false;
				if (mesh.GetComponent<Renderer>().bounds.min.y > UpperBoundary.bounds.min.y) {
					if (mesh.GetComponent<Renderer>().bounds.min.y < UpperBoundary.bounds.max.y) {
						float distance = UpperBoundary.bounds.max.y - UpperBoundary.bounds.min.y;
						float value = mesh.GetComponent<Renderer>().bounds.min.y - UpperBoundary.bounds.min.y;
						float percent = value / distance;
						if (z == SelectedIndex) {
							newColor = Color.Lerp(this.ItemActiveColor,this.ItemInvisibleColor,percent);
						}
						else {
							newColor = Color.Lerp(this.ItemInactiveColor,this.ItemInvisibleColor,percent);
						}
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
						if (z == SelectedIndex) {
							newColor = Color.Lerp(this.ItemActiveColor,this.ItemInvisibleColor,percent);
						}
						else {
							newColor = Color.Lerp(this.ItemInactiveColor,this.ItemInvisibleColor,percent);
						}
					}
					else
					{
						newColor = this.ItemInvisibleColor;
					}
				}
				else {
					isItemTouchable = true;
					if (z == SelectedIndex) {
						newColor = this.ItemActiveColor;
					}
					else {
						newColor = this.ItemInactiveColor;
					}
				}
				
				ComboItems[z].TextObject.color = newColor;
				ComboItems[z].BoundingCollider.enabled = isItemTouchable;
			}
		}
		if (!lastPulloutActive) {
			TogglePulloutActive();
		}
	}

	void HandleSelectedIndexChanged (object sender, System.EventArgs e)
	{
		this.SetItemText();
		TriggerSelectionChanged();
	}

	void HandleBoxClick (object sender, System.EventArgs e)
	{
		this.TogglePulloutActive();
	}

	void HandleClickUpRight (object sender, System.EventArgs e)
	{
		if (this.DataValues == null) return;
		if (this.DataValues.Rows != null) {
			if (this.SelectedIndex < this.DataValues.Rows.Length - 1) {
				this.SelectedIndex++;
				this.SetItemText();
				this.TriggerSelectionChanged();
				this.SetItemVisibility();
			}
		}
	}

	void HandleClickUpLeft (object sender, System.EventArgs e)
	{
		if (this.DataValues == null) return;
		if (this.DataValues.Rows != null) {
			if (this.SelectedIndex > 0) {
				this.SelectedIndex--;
				this.SetItemText();
				this.TriggerSelectionChanged();
				this.SetItemVisibility();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetItemText() {
		if (DataValues != null) {
			if (DataValues.Rows.Length > SelectedIndex) {
				this.ItemText.text = DataValues.Rows[SelectedIndex][DisplayMember].PropertyValue;
			}
		}
	}

	public string GetItemValue() {
		return DataValues.Rows[SelectedIndex][ValueMember].PropertyValue;
	}

	public string GetItemText() {
		return DataValues.Rows[SelectedIndex][DisplayMember].PropertyValue;
	}

	public void TriggerSelectionChanged() {
		if (this.SelectedIndexChanged != null) {
			this.SelectedIndexChanged(null, new System.EventArgs());
		}
	}
}
