using UnityEngine;
using System.Collections;

public class VirtualUIDatePicker : MonoBehaviour {

	public string CurrentDateString;
	public System.DateTime CurrentDate;
	public VirtualUIButton PrevMonth;
	public VirtualUIButton NextMonth;
	public TextMesh MonthName;
	public Transform[] WeekObjects;
	public VirtualUIDatePickerDay[] DayObjects;
	public Color ActiveColor = Color.cyan;
	public Color InactiveColor = Color.white;
	public VirtualUIButton ComboButton;
	public VirtualUIButton ComboNext;
	public VirtualUIButton ComboPrev;
	public VirtualUIButton TodayButton;
	public TextMesh ComboItemText;
	public Transform DatePullout;
	public VirtualUITimePicker TimePicker;

	// Use this for initialization
	void Start () {
		DayObjects = new VirtualUIDatePickerDay[42];
		int i = 0;
		for (int z=0;z<6;z++) {
			Transform child = WeekObjects[z];
			for (int y=1;y<=7;y++) {
				Transform child1 = child.GetChild(y-1);
				VirtualUIDatePickerDay day = (VirtualUIDatePickerDay)child1.GetComponent(typeof(VirtualUIDatePickerDay));
				day.Button.ClickUp += HandleDayClick;
				DayObjects[i] = day;

				i++;
			}
		}

		this.SetCurrentDate();
		if (this.TimePicker != null) {
			this.TimePicker.SetNewTime(this.CurrentDate.Hour, this.CurrentDate.Minute, this.CurrentDate.Second);
		}
		this.BuildCurrentCalendarMonth();
		this.PrevMonth.ClickUp += HandlePreviousMonthClick;
		this.NextMonth.ClickUp += HandleNextMonthClick;

		this.ComboButton.ClickUp += HandleComboClickUp;
		this.ComboNext.ClickUp += HandleComboNextClickUp;
		this.ComboPrev.ClickUp += HandleComboPrevClickUp;
		this.TodayButton.ClickUp += HandleTodayClickUp;

		if (this.TimePicker != null) {
			this.TimePicker.TimeUpdated += HandleTimePickerUpdate;
		}
	}

	void UpdateFromTimePicker() {
		if (this.TimePicker != null) {
			System.TimeSpan span = new System.TimeSpan();
			if (this.TimePicker.Use24HourClock) {
				span = new System.TimeSpan(this.TimePicker.Hours, this.TimePicker.Minutes, this.TimePicker.Seconds);
			}
			else {
				//Debug.Log(this.TimePicker.AMPM + " / " + this.TimePicker.Hours);
				span = new System.TimeSpan(this.TimePicker.Convert12to24Hour(this.TimePicker.Hours, this.TimePicker.AMPM), this.TimePicker.Minutes, this.TimePicker.Seconds);
				//Debug.Log(span.Hours);
			}
			
			this.CurrentDate = this.CurrentDate.Date;
			this.CurrentDate = this.CurrentDate.Add(span);
		}
	}

	void HandleTimePickerUpdate (object sender, System.EventArgs e)
	{
		this.UpdateFromTimePicker();
		this.CurrentDateString = this.CurrentDate.ToString();
		this.SetCurrentDate();
	}

	void HandleTodayClickUp (object sender, System.EventArgs e)
	{
		if (this.TimePicker != null) {
			this.CurrentDate = System.DateTime.Now.Date;
		}
		else {
			this.CurrentDate = System.DateTime.Now.Date;
		}
		this.UpdateFromTimePicker();
		this.CurrentDateString = this.CurrentDate.ToString();
		this.SetCurrentDate();
		this.BuildCurrentCalendarMonth();
	}

	void HandleComboPrevClickUp (object sender, System.EventArgs e)
	{
		this.CurrentDate = this.CurrentDate.AddDays(-1);
		this.UpdateFromTimePicker();
		this.CurrentDateString = this.CurrentDate.ToString();
		this.SetCurrentDate();
		this.BuildCurrentCalendarMonth();
	}

	void HandleComboNextClickUp (object sender, System.EventArgs e)
	{
		this.CurrentDate = this.CurrentDate.AddDays(1);
		this.UpdateFromTimePicker();
		this.CurrentDateString = this.CurrentDate.ToString();
		this.SetCurrentDate();
		this.BuildCurrentCalendarMonth();
	}

	void HandleComboClickUp (object sender, System.EventArgs e)
	{
		this.DatePullout.gameObject.SetActive(!this.DatePullout.gameObject.activeSelf);
		if (this.TimePicker != null) {
			this.TimePicker.gameObject.SetActive(!this.TimePicker.gameObject.activeSelf);
		}
	}

	void HandleNextMonthClick (object sender, System.EventArgs e)
	{
		this.CurrentDate = this.CurrentDate.AddMonths(1);
		this.UpdateFromTimePicker();
		this.CurrentDateString = this.CurrentDate.ToString();
		this.SetCurrentDate();
		this.BuildCurrentCalendarMonth();
	}

	void HandlePreviousMonthClick (object sender, System.EventArgs e)
	{
		this.CurrentDate = this.CurrentDate.AddMonths(-1);
		this.UpdateFromTimePicker();
		this.CurrentDateString = this.CurrentDate.ToString();
		this.SetCurrentDate();
		this.BuildCurrentCalendarMonth();
	}

	void HandleDayClick (object sender, System.EventArgs e)
	{
		VirtualUIButton obj = (VirtualUIButton)sender;
		for (int z=0;z<42;z++) {
			if (this.DayObjects[z].Button == obj) {
				this.CurrentDate = this.DayObjects[z].DateValue;
			}
		}
		this.UpdateFromTimePicker();
		this.CurrentDateString = this.CurrentDate.ToString();
		this.HighlightSelectedDay();
		this.SetCurrentDate();
	}

	void HighlightSelectedDay() {
		for (int z=0;z<42;z++) {
			Color col = Color.white;
			if (this.DayObjects[z].DateValue == this.CurrentDate.Date)
			{
				col = this.ActiveColor;
			}
			else {
				col = this.InactiveColor;
			}
			//this.DayObjects[z].Button.spriteRenderer.color = col;
			this.DayObjects[z].Text.color = col;
		}
	}

	void SetCurrentDate() {
		try
		{
			if (CurrentDateString.Trim() != string.Empty) {
				this.CurrentDate = System.DateTime.Parse(CurrentDateString);
			}
			else {
				this.CurrentDate = System.DateTime.Now.Date;
			}
		}
		catch
		{
			this.CurrentDate = System.DateTime.Now.Date;
		}
		if (this.TimePicker != null) {

			try
			{
				if (CurrentDateString.Trim() != string.Empty) {
					this.CurrentDate = System.DateTime.Parse(CurrentDateString);
				}
				else {
					this.CurrentDate = System.DateTime.Now;
				}
			}
			catch(System.Exception ex)
			{
				this.CurrentDate = System.DateTime.Now;
			}
		}
		this.CurrentDateString = this.CurrentDate.ToString();
		this.SetComboText();
	}

	void SetComboText() {
		if (this.TimePicker != null) {
			this.ComboItemText.text = this.CurrentDate.ToString("ddd M/dd/yyyy hh:mm:ss tt");
		}
		else 
			this.ComboItemText.text = this.CurrentDate.ToString("ddd M/dd/yyyy");
	}

	void BuildCurrentCalendarMonth() {
		System.DateTime startDate = new System.DateTime(this.CurrentDate.Year, this.CurrentDate.Month, 1);
		System.DateTime endDate = startDate.AddMonths(1).AddDays(-1);

		this.MonthName.text = startDate.ToString("MMMM yyyy");

		System.DateTime iDate = startDate;
		int week = 0;
		int dow = 1;
		for (int z=0;z<42;z++) {
			this.DayObjects[z].gameObject.SetActive(true);
		}
		int iter = 0;
		while (iDate <= endDate) {
			dow = (int)iDate.DayOfWeek; // 1-7 Sun-Sat
			if (iDate == startDate) {
				if (dow > 0) {
					while (iter < dow) {
						this.DayObjects[iter].DateValue = new System.DateTime(1900,1,1);
						this.DayObjects[iter].gameObject.SetActive(false);
						iter++;
					}
				}
			}
			this.DayObjects[iter].Text.text = iDate.Day.ToString();
			Color col = (iDate == this.CurrentDate.Date) ? this.ActiveColor : this.InactiveColor;
			this.DayObjects[iter].DateValue = iDate;
			this.DayObjects[iter].Text.color = col;
			//this.DayObjects[iter].Button.spriteRenderer.color = col;
			iter++;

			if (dow == 6) {
				week++;
			}
			iDate = iDate.AddDays(1);
			if (iDate > endDate) {
				for (int z=iter;z<42;z++) {
					this.DayObjects[iter].DateValue = new System.DateTime(1900,1,1);
					this.DayObjects[z].gameObject.SetActive(false);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
