using UnityEngine;
using System.Collections;

public class RepeaterRow : MonoBehaviour {

	public RepeaterItemValue[] ItemValues;

	public RepeaterItemValue this[string index] {
		get {
			for (int z=0;z<ItemValues.Length;z++) {
				if (ItemValues[z].PropertyName == index)
					return ItemValues[z];
			}
			return null;
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override string ToString ()
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		foreach(RepeaterItemValue item in ItemValues) {
			sb.AppendLine(item.ToString());
		}
		return sb.ToString();
	}
}
