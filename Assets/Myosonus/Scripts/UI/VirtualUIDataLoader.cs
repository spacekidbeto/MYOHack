using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VirtualUIDataLoader : MonoBehaviour {

	public VirtualUIJSONDownloader JSONDownloader;
	public event System.EventHandler RowsLoaded;
	public RepeaterRowInfo [] RowInfo;
	public List<RepeaterRow> Rows;
	public IVirtualUIRowFilter RowFilter;
	public int RowsBetweenYields = 1;

	bool StartDataLoad = false;

	// Use this for initialization
	void Start () {
		JSONDownloader.DownloadDoneEvent += HandleDownloadDoneEvent;
	}

	void HandleDownloadDoneEvent (object sender, System.EventArgs e)
	{
		StartDataLoad = true;
	}

	public void ReloadData() {
		this.JSONDownloader.StatusText.StartFadeIn();
		StartDataLoad = true;
	}

	void UpdateText(string text) {
		if (this.JSONDownloader == null) return;
		if (this.JSONDownloader.StatusText == null) return;
		this.JSONDownloader.StatusText.TextObject.text = text;
	}

	IEnumerator LoadDataFrame() {
		int nodeCount = this.JSONDownloader.DownloadedNode.Count;
		for (int z=0;z<nodeCount;z++) {
			SimpleJSON.JSONNode node = this.JSONDownloader.DownloadedNode[z];
			if (node.Count > 1) {
				List<RepeaterItemValue> items = new List<RepeaterItemValue>();
				for (int y=0;y<RowInfo.Length;y++) {
					items.Add(new RepeaterItemValue() {
						PropertyName = RowInfo[y].PropertyName,
						PropertyType = RowInfo[y].PropertyType,
						PropertyValue = node[RowInfo[y].DataRowIndex]
					});
				}
				RepeaterRow row = new RepeaterRow();
				row.ItemValues = items.ToArray();
				if (this.RowFilter != null) {
					bool filter = this.RowFilter.FilterRow(row);
					if (filter) {
						this.Rows.Add(row);
					}
				}
				else
				{
					this.Rows.Add(row);
				}
			}
			this.UpdateText(string.Format("Loading Row {0}/{1}...", z, nodeCount));
			if (z % this.RowsBetweenYields == 0)
				yield return null;
		}
		if (this.RowsLoaded != null) {
			this.RowsLoaded(this, new System.EventArgs());
		}
		this.UpdateText("Loading Completed...");
		this.JSONDownloader.StatusText.StartFadeOut();
	}
	
	// Update is called once per frame
	void Update () {
		if (StartDataLoad) {
			this.Rows.Clear();
			StartDataLoad = false;
			StartCoroutine("LoadDataFrame");
		}
	}
}
