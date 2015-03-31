using UnityEngine;
using System.Collections;

public class VirtualUITimePicker : MonoBehaviour {

	public System.DateTime CurrentDateTime;
	public string CurrentDateTimeString;

	public VirtualUIButton HoursUpButton;
	public VirtualUIButton HoursDownButton;
	public VirtualUIButton MinutesUpButton;
	public VirtualUIButton MinutesUp5Button;
	public VirtualUIButton MinutesDownButton;
	public VirtualUIButton MinutesDown5Button;
	public VirtualUIButton SecondsUpButton;
	public VirtualUIButton SecondsUp5Button;
	public VirtualUIButton SecondsDownButton;
	public VirtualUIButton SecondsDown5Button;
	public VirtualUIButton AMPMUpButton;
	public VirtualUIButton AMPMDownButton;
	public event System.EventHandler TimeUpdated;

	public TextMesh HoursText;
	public TextMesh MinutesText;
	public TextMesh SecondsText;
	public TextMesh AMPMText;

	public int Hours = 12;
	public int Minutes = 0;
	public int Seconds = 0;
	public int AMPM = 0;
	public bool Use24HourClock = false;

	// Use this for initialization
	void Start () {
		this.HoursUpButton.ClickUp += HandleHoursUp;
		this.HoursDownButton.ClickUp += HandleHoursDown;
		this.MinutesUpButton.ClickUp += HandleMinutesUp;
		this.MinutesUp5Button.ClickUp += HandleMinutesUp5;
		this.MinutesDownButton.ClickUp += HandleMinutesDown;
		this.MinutesDown5Button.ClickUp += HandleMinutesDown5;
		this.SecondsUpButton.ClickUp += HandleSecondsUp;
		this.SecondsUp5Button.ClickUp += HandleSecondsUp5;
		this.SecondsDownButton.ClickUp += HandleSecondsDown;
		this.SecondsDown5Button.ClickUp += HandleSecondsDown5;
		this.AMPMUpButton.ClickUp += HandleAMPM;
		this.AMPMDownButton.ClickUp += HandleAMPM;
		this.UpdateTime();
	}

	public System.TimeSpan GetValueAsTimeSpan() {
		int hours = this.Hours;
		if (this.Use24HourClock) {
			hours = this.Hours;
		}
		else {
			if (hours > 12) { hours -= 12; AMPM = 1; }
			else if (hours == 0) { hours = 12; AMPM = 0; }
			else if (hours == 12 && AMPM == 1) { hours = 12; AMPM = 1; }
			else if (hours == 12 && AMPM == 0) { hours = 12; AMPM = 0; }
			else { AMPM = 0; }
		}

		System.TimeSpan span = new System.TimeSpan(hours, this.Minutes, this.Seconds);
		return span;
	}

	void HandleAMPM (object sender, System.EventArgs e)
	{
		if (this.AMPM == 0) this.AMPM = 1;
		else this.AMPM = 0;
		this.UpdateTime();
	}

	void HandleSecondsDown5 (object sender, System.EventArgs e)
	{
		this.Seconds-=5;
		if (this.Seconds < 0) this.Seconds = 59;
		this.UpdateTime();
	}

	void HandleSecondsDown (object sender, System.EventArgs e)
	{
		this.Seconds--;
		if (this.Seconds < 0) this.Seconds = 59;
		this.UpdateTime();
	}

	void HandleSecondsUp5 (object sender, System.EventArgs e)
	{
		this.Seconds+=5;
		if (this.Seconds > 59) this.Seconds = 0;
		this.UpdateTime();
	}

	void HandleSecondsUp (object sender, System.EventArgs e)
	{
		this.Seconds++;
		if (this.Seconds > 59) this.Seconds = 0;
		this.UpdateTime();
	}

	void HandleMinutesDown5 (object sender, System.EventArgs e)
	{
		this.Minutes-=5;
		if (this.Minutes < 0) this.Minutes = 59;
		this.UpdateTime();
	}

	void HandleMinutesDown (object sender, System.EventArgs e)
	{
		this.Minutes--;
		if (this.Minutes < 0) this.Minutes = 59;
		this.UpdateTime();
	}

	void HandleMinutesUp5 (object sender, System.EventArgs e)
	{
		this.Minutes+=5;
		if (this.Minutes > 59) this.Minutes = 0;
		this.UpdateTime();
	}

	void HandleMinutesUp (object sender, System.EventArgs e)
	{
		this.Minutes++;
		if (this.Minutes > 59) this.Minutes = 0;
		this.UpdateTime();
	}

	void HandleHoursDown (object sender, System.EventArgs e)
	{
		this.Hours--;
		if (!this.Use24HourClock) {
			if (this.Hours < 1) this.Hours = 12;
		}
		else {
			if (this.Hours < 0) this.Hours = 23;
		}
		if (!this.Use24HourClock) {
			if (this.Hours > 12) this.Hours = 1;
		}
		else {
			if (this.Hours > 23) this.Hours = 0;
		}
		this.UpdateTime();
	}

	void HandleHoursUp (object sender, System.EventArgs e)
	{
		this.Hours++;
		if (!this.Use24HourClock) {
			if (this.Hours < 1) this.Hours = 12;
		}
		else {
			if (this.Hours < 0) this.Hours = 23;
		}
		if (!this.Use24HourClock) {
			if (this.Hours > 12) this.Hours = 1;
		}
		else {
			if (this.Hours > 23) this.Hours = 0;
		}
		this.UpdateTime();
	}

	public int Convert12to24Hour(int hours, int ampm) {
		int newHour = 0;
		if (ampm == 0) {
			if (hours == 12) newHour = 0;
			else newHour = hours;
		}
		else {
			if (hours == 12) newHour = 12;
			else newHour = hours + 12;
		}
		return newHour;
	}

	public void SetNewTime(int hours, int minutes, int seconds) {
		if (this.Use24HourClock) {
			this.Hours = hours;
		}
		else {
			if (hours > 12) { hours -= 12; AMPM = 1; }
			else if (hours == 0) { hours = 12; AMPM = 0; }
			else if (hours == 12 && AMPM == 1) { hours = 12; AMPM = 1; }
			else if (hours == 12 && AMPM == 0) { hours = 12; AMPM = 0; }
			else { AMPM = 0; }
			this.Hours = hours;
		}
		this.Minutes = minutes;
		this.Seconds = seconds;
		this.UpdateTime();
	}

	public void UpdateTime() {
		this.HoursText.text = this.Hours.ToString();
		this.MinutesText.text = this.Minutes.ToString().PadLeft(2,'0');
		this.SecondsText.text = this.Seconds.ToString().PadLeft(2,'0');
		if (!this.Use24HourClock) {
			if (!this.AMPMUpButton.gameObject.activeSelf) this.AMPMUpButton.gameObject.SetActive(true);
			if (!this.AMPMDownButton.gameObject.activeSelf) this.AMPMDownButton.gameObject.SetActive(true);
			if (!this.AMPMText.gameObject.activeSelf) this.AMPMText.gameObject.SetActive(true);
			if (this.AMPM == 0) this.AMPMText.text = "AM";
			else this.AMPMText.text = "PM";
		}
		else {
			if (this.AMPMUpButton.gameObject.activeSelf) this.AMPMUpButton.gameObject.SetActive(false);
			if (this.AMPMDownButton.gameObject.activeSelf) this.AMPMDownButton.gameObject.SetActive(false);
			if (this.AMPMText.gameObject.activeSelf) this.AMPMText.gameObject.SetActive(false);
		}
		if (this.TimeUpdated != null) {
			this.TimeUpdated(this, new System.EventArgs());
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
