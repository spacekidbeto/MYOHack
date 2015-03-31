using UnityEngine;
using System.Collections;
using System;
using System.Text;

public class AlarmTimeInfo : MonoBehaviour {

	public enum AMPM
	{
		AM,
		PM
	}

	public int Hour;
	public int Minute;
	public AMPM AMorPM;
	public DayOfWeek[] Days;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	string GetDayString(DayOfWeek day) {
		bool found = false;
		foreach(DayOfWeek alarmDay in Days) {
			if (alarmDay == day) {
				found = true;
			}
		}
		return found ? day.ToString()[0].ToString() : "-";
	}

	public override string ToString ()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(GetDayString(DayOfWeek.Sunday));
		sb.Append(GetDayString(DayOfWeek.Monday));
		sb.Append(GetDayString(DayOfWeek.Tuesday));
		sb.Append(GetDayString(DayOfWeek.Wednesday));
		sb.Append(GetDayString(DayOfWeek.Thursday));
		sb.Append(GetDayString(DayOfWeek.Friday));
		sb.Append(GetDayString(DayOfWeek.Saturday));

		return string.Format ("{0:D2}:{1:D2} {2} - {3}",Hour,Minute,AMorPM,sb.ToString());
	}
}
