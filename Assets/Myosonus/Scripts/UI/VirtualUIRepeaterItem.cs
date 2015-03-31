using UnityEngine;
using System.Collections;

public class VirtualUIRepeaterItem : MonoBehaviour {

	public string DataValue;
	public string DataMember;
	public string DataFormat;
	public string DataType;
	public TextMesh DataTextMesh;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void UpdateText() {
		if (DataFormat != string.Empty) {
			try
			{
			switch(DataType.ToLower()) {
			case "string":
				this.DataTextMesh.text = string.Format(DataFormat,DataValue); 
				break;
			case "int": 
				long outInt = System.Int64.Parse(this.DataValue); 
				this.DataTextMesh.text = string.Format(DataFormat,outInt); 
				break;
			case "float":
				decimal outFloat = System.Decimal.Parse(this.DataValue); 
				this.DataTextMesh.text = string.Format(DataFormat,outFloat); 
				break;
			case "datetime":
				System.DateTime outDate = System.DateTime.Parse(this.DataValue); 
				this.DataTextMesh.text = string.Format(DataFormat,outDate); 
				break;
			default:break;
			}
			}
			catch(System.Exception ex) {
				Debug.Log(this.ToString() + " / " + ex.ToString());
			}
		}
		else {
			this.DataTextMesh.text = DataValue;
		}

	}

	public override string ToString() {
		return  this.DataMember + " / " +  this.DataFormat + " / " + this.DataValue + " / " + this.DataType;
	}
}
