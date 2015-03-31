using UnityEngine;
using System.Collections;

public class ShowAlarmTime : MonoBehaviour {

	public AlarmTimeInfo Info;
	public TextMesh Text;
	// Use this for initialization
	void Start () {
		this.Text.text = Info.ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
