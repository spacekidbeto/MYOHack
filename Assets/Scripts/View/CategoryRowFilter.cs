using UnityEngine;
using System.Collections;

public class CategoryRowFilter : IVirtualUIRowFilter {

	public int CategoryId = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool FilterRow(RepeaterRow inputRow) {
		bool found = false;
		foreach(RepeaterItemValue item in inputRow.ItemValues) {
			if (item.PropertyName == "CategoryId") {
				int value = 0;
				System.Int32.TryParse(item.PropertyValue, out value);
				if (value == CategoryId)
				{
					found = true;
				}
			}
		}
		return found;
	} 
}
