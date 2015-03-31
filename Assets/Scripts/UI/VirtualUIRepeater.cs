using UnityEngine;
using System.Collections;
using System;

public class VirtualUIRepeater : MonoBehaviour {

	public enum RowDirection
	{
		XMinus,
		X,
		YMinus,
		Y,
		ZMinus,
		Z
	}

	public RowDirection RepeaterRowDirection;

	public RepeaterRowCollection Rows = null;

	public int RowsPerPage = 6;
	public int TotalRecords {
		get {
			if (Rows == null) return 0;
			if (Rows.Rows == null) return 0;
			return Rows.Rows.Length;
		}
	}
	public float SpaceBetweenRows = 0.1f;
	public int CurrentPage = 1;
	public VirtualUIButton NextPageButton = null;
	public VirtualUIButton PreviousPageButton = null;
	public TextMesh PageTextMesh = null;
	public string PageTextFormat = "Page {0} of {1}, {2} Records";

	public GameObject TemplateObject = null;
	bool shown = false;
	

	public int TotalPages {
		get {
			if (TotalRecords == 0) return 0;
			int floor = TotalRecords / RowsPerPage;
			int last = (TotalRecords % RowsPerPage > 0) ? 1 : 0;
			return floor + last;
		}
	}

	public VirtualUIDataLoader Loader;
	public bool StartLoaderAutomatically = true;

	// Use this for initialization
	void Start () {
		this.NextPageButton.ClickUp += HandleNextClickUp;
		this.PreviousPageButton.ClickUp += HandlePreviousClickUp;
		this.UpdateRepeater();

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

	public void RefreshLoadedData() {
		if (Loader != null) {
			if (Loader.JSONDownloader.DownloadDone) {
				Loader.ReloadData();
			}
			else {
				Loader.JSONDownloader.StartDownload();
			}
		}
	}

	void HandleRowsLoaded (object sender, EventArgs e)
	{
		if (this.Rows != null)
		{
			this.KillPreviousItems();
			Destroy(this.Rows);
		}
		this.Rows = (RepeaterRowCollection)this.gameObject.AddComponent(typeof(RepeaterRowCollection));
		this.Rows.Rows = this.Loader.Rows.ToArray();
		this.UpdateRepeater();
	}

	void SetPageText() {
		if (this.PageTextMesh != null) {
			this.PageTextMesh.text = string.Format(this.PageTextFormat,this.CurrentPage,this.TotalPages,this.TotalRecords);
		}
	}

	public void KillPreviousItems() {
		for (int z=this.transform.childCount-1;z>=0;z--) {
			Transform child = this.transform.GetChild(z);
			if (child.name.StartsWith("ItemRow_")) {
				var faderItems = child.gameObject.GetComponentsInChildren(typeof(VirtualUIFadableText));
				foreach(VirtualUIFadableText fader in faderItems) {
					fader.KillOnFadeOut = true;
					fader.StartFadeOut();
				}
			}
		}
	}

	void HandlePreviousClickUp (object sender, EventArgs e)
	{
		if (this.CurrentPage > 1) {
			this.CurrentPage--;
		}
		this.KillPreviousItems();
		this.UpdateRepeater();
	}

	void HandleNextClickUp (object sender, EventArgs e)
	{
		if (this.CurrentPage < this.TotalPages) {
			this.CurrentPage++;
		}
		this.KillPreviousItems();
		this.UpdateRepeater();
	}

	public void UpdateRepeater() {
		if (this.Rows == null) {
			return;
		}
		this.SetPageText();
		//TemplateObject.gameObject.SetActive(true);
		//TemplateObject.gameObject.SetActive(false);
		//this.TemplateObject.gameObject.SetActive(true);
		Vector3 startPosition = this.TemplateObject.transform.localPosition;

		int startValue = (CurrentPage - 1) * this.RowsPerPage;
		int endValue = (CurrentPage) * this.RowsPerPage - 1;
		if (endValue > this.Rows.Rows.Length) {
			endValue = this.Rows.Rows.Length - 1;
		}
		int rowNumber = 0;
		for(int z=startValue;z<=endValue;z++) {
			try
			{
				RepeaterRow row = this.Rows.Rows[z];
				Transform myTrans = this.TemplateObject.transform;

				Transform newObject = (Transform)Instantiate(myTrans, startPosition, myTrans.rotation);
				newObject.transform.parent = this.transform;
				newObject.transform.localPosition = startPosition;
				newObject.transform.name = "ItemRow_" + rowNumber;

				var repeaterItems = newObject.gameObject.GetComponentsInChildren(typeof(VirtualUIRepeaterItem));
				var faderItems = newObject.gameObject.GetComponentsInChildren(typeof(VirtualUIFadableText));
				/*
				int visRow = 0;
				for(int z=startValue;z<=endValue;z++) {
					JohnnyUIRepeaterItem item = (JohnnyUIRepeaterItem)repeaterItems[z];
				}
				*/
				foreach(VirtualUIRepeaterItem item in repeaterItems) {
					try
					{
						foreach(RepeaterItemValue value in row.ItemValues) {
							if (item.DataMember.ToLower() == value.PropertyName.ToLower()) {
								item.DataValue = value.PropertyValue;
								item.DataType = value.PropertyType.ToString();
								item.UpdateText();
							}
						}
					}
					catch(System.Exception ex) {
						foreach(RepeaterItemValue val in row.ItemValues) {
							Debug.Log(val.PropertyName + " / " + val.PropertyValue);
						}
					}
				}
				foreach(VirtualUIFadableText fader in faderItems) {
					fader.StartFadeIn();
				}

				startPosition = this.IncrementDirection(startPosition);
				rowNumber++;
			}
			catch(System.Exception ex) {
				Debug.Log(ex.ToString());
			}
		}
		//this.TemplateObject.gameObject.SetActive(false);
	}

	Vector3 IncrementDirection(Vector3 input) {
		switch(this.RepeaterRowDirection) {
		case RowDirection.X:input.x += this.SpaceBetweenRows; break;
		case RowDirection.XMinus:input.x -= this.SpaceBetweenRows;break;
		case RowDirection.Y:input.y += this.SpaceBetweenRows;break;
		case RowDirection.YMinus:input.y -= this.SpaceBetweenRows;break;
		case RowDirection.Z:input.z += this.SpaceBetweenRows;break;
		case RowDirection.ZMinus:input.z -= this.SpaceBetweenRows;break;
		default:break;
		}
		return input;
	}
	
	// Update is called once per frame
	void Update () {
		/*
		if (!this.shown) {
			Debug.Log("Here");
			this.shown = true;
			this.ShowRepeater();
		}
		*/
	}
}
