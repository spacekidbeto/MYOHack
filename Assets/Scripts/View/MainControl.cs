using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class MainControl : MonoBehaviour {

	// Use this for initialization
	public float SecondsBeforeMeltdown = 5f;
	bool triggered = false;
	public VirtualUIBarChart barChart;
	public VirtualUIAreaChart areaChart;
	public VirtualUIPieChart pieChart;

	public RepeaterRowCollection AllTransactionsDataValues;
	public VirtualUIRepeater AllTransactionsRepeater;

	public RepeaterRowCollection PMNameDataValues;
	public VirtualUIRepeater PMNameRepeater;

	public RepeaterRowCollection PMNameCatDataValues;
	public VirtualUIRepeater PMNameCatRepeater;

	public RepeaterRowCollection WeeklyDataValues;
	public VirtualUIRepeater WeeklyRepeater;

	public RepeaterRowCollection MonthlyDataValues;
	public VirtualUIRepeater MonthlyRepeater;

	string inputText = @"This is some text";

	public VirtualUIComboBox CategoryBox;

	private WWW webDownloadAllTransactions;
	private WWW webDownloadPMName;
	private WWW webDownloadPMNameCat;
	private WWW webDownloadWeekly;
	private WWW webDownloadMonthly;

	bool allTransactionsWebDone = false;
	bool pmNameWebDone = false;
	bool pmNameCatWebDone = false;
	bool weeklyWebDone = false;
	bool monthlyWebDone = false;

	string webStringAllTransactions;
	string webStringPMName;
	string webStringPMNameCat;
	string webStringWeekly;
	string webStringMonthly;

	SimpleJSON.JSONNode webJSONAllTransactions;
	SimpleJSON.JSONNode webJSONPMName;
	SimpleJSON.JSONNode webJSONPMNameCat;
	SimpleJSON.JSONNode webJSONWeekly;
	SimpleJSON.JSONNode webJSONMonthly;


	WWW webDownloadCategories;
	bool categoriesWebDone = false;
	string webStringCategories;
	SimpleJSON.JSONNode webJSONCategories;

	void Awake() {
		Application.targetFrameRate = 60;
		//if (Application.platform != RuntimePlatform.MetroPlayerX86) {

		//Screen.SetResolution(1280,720,true);

		//}
		//if (Application.platform == RuntimePlatform.WindowsPlayer) {
		//	Input.simulateMouseWithTouches = true;
		//}
		Input.multiTouchEnabled = true;
		
	}
	void Start () {
		try
		{
			this.PMNameCatRepeater.Loader.RowFilter = new CategoryRowFilter() {CategoryId = 0};
			this.MonthlyRepeater.Loader.RowFilter = new CategoryRowFilter() {CategoryId = 0};
			this.WeeklyRepeater.Loader.RowFilter = new CategoryRowFilter() {CategoryId = 0};

			this.CategoryBox.SelectedIndexChanged += HandleCategoryChanged;
			this.CategoryBox.Loader.RowsLoaded += HandleRowsLoaded;
			//LoadDB();
			//webDownload = new WWW(@"http://localhost/ExpenseTrackerWebServer/api/CustomViews/MonthlySpendingByCategory?userId=1");

			/*
			webDownloadAllTransactions = new WWW(@"http://localhost/ExpenseTrackerWebServer/api/AllTransactions");
			webDownloadPMName = new WWW(@"http://localhost/ExpenseTrackerWebServer/api/CustomViews/PriceMatrixByName?userId=1");
			webDownloadPMNameCat = new WWW(@"http://localhost/ExpenseTrackerWebServer/api/CustomViews/PriceMatrixByNameByCategory?userId=1");
			webDownloadWeekly = new WWW(@"http://localhost/ExpenseTrackerWebServer/api/CustomViews/WeeklySpendingByCategory?userId=1");
			webDownloadMonthly = new WWW(@"http://localhost/ExpenseTrackerWebServer/api/CustomViews/MonthlySpendingByCategory?userId=1");

			webDownloadCategories = new WWW(@"http://localhost/ExpenseTrackerWebServer/api/TransactionCategories?userId=1");
			*/
		}
		catch(System.Exception ex) {
			//this.inputText = ex.ToString();
		}
	}

	void HandleRowsLoaded (object sender, System.EventArgs e)
	{

		// TO DO: Figure out how to load conditions into the repeaters.
		/*
		this.PMNameCatRepeater.StartLoader();
		this.MonthlyRepeater.StartLoader();
		this.WeeklyRepeater.StartLoader();
		*/
	}

	void HandleCategoryChanged (object sender, System.EventArgs e)
	{
		//Debug.Log("NEW ITEM: " + this.CategoryBox.GetItemValue() + " / " + this.CategoryBox.GetItemText());
		int newCategoryId = System.Convert.ToInt32(this.CategoryBox.GetItemValue());

		(this.PMNameCatRepeater.Loader.RowFilter as CategoryRowFilter).CategoryId = newCategoryId;
		(this.MonthlyRepeater.Loader.RowFilter as CategoryRowFilter).CategoryId = newCategoryId;
		(this.WeeklyRepeater.Loader.RowFilter as CategoryRowFilter).CategoryId = newCategoryId;
		this.PMNameCatRepeater.RefreshLoadedData();
		this.MonthlyRepeater.RefreshLoadedData();
		this.WeeklyRepeater.RefreshLoadedData();
		// TO DO: Figure out how to load conditions into the repeaters.

		/*
		this.LoadMonthlySpendingReport(newCategoryId);
		this.LoadWeeklySpendingReport(newCategoryId);
		this.LoadPMByNameCatReport(newCategoryId);
		*/
	}

	void OnGUI() {
		//this.inputText = GUI.TextField(new Rect(0f,0f,200f,30f),this.inputText);
	}

	void LoadAllTransactionsReport() {
		if (this.AllTransactionsDataValues != null) {
			this.AllTransactionsRepeater.KillPreviousItems();
			Destroy(this.AllTransactionsDataValues);
		}

		this.AllTransactionsDataValues = (RepeaterRowCollection)this.gameObject.AddComponent(typeof(RepeaterRowCollection));
		List<RepeaterRow> rows = new List<RepeaterRow>();

		//SELECT BusinessName, ReceiptDate, ItemName, ItemAmount from AllTransactions order by ReceiptDate, BusinessName, ItemName;
		for (int z=0;z<this.webJSONAllTransactions.Count;z++) {
			SimpleJSON.JSONNode node = this.webJSONAllTransactions[z];
			// ReceiptDate - 3
			// BusineessName - 6
			// ItemName - 7
			// ItemAmount - 8

			List<RepeaterItemValue> items = new List<RepeaterItemValue>();
			RepeaterItemValue value1 = new RepeaterItemValue() {
				PropertyName = "ReceiptDate", PropertyType = RepeaterItemValue.PropertyTypes.DateTime, PropertyValue = node[3].Value
			};
			RepeaterItemValue value2 = new RepeaterItemValue() {
				PropertyName = "BusinessName", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[6].Value
			};
			RepeaterItemValue value3 = new RepeaterItemValue() {
				PropertyName = "ItemName", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[7].Value
			};
			RepeaterItemValue value4 = new RepeaterItemValue() {
				PropertyName = "ItemAmount", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[8].Value
			};
			items.AddRange(new RepeaterItemValue[] {value1,value2,value3,value4});
			RepeaterRow row = new RepeaterRow();
			row.ItemValues = items.ToArray();
			rows.Add(row);
		}

		this.AllTransactionsDataValues.Rows = rows.ToArray();
		this.AllTransactionsRepeater.Rows = this.AllTransactionsDataValues;
		this.AllTransactionsRepeater.UpdateRepeater();
	}

	void LoadPMByNameReport() {
		if (this.PMNameDataValues != null)
		{
			this.PMNameRepeater.KillPreviousItems();
			Destroy(this.PMNameDataValues);
		}

		this.PMNameDataValues = (RepeaterRowCollection)this.gameObject.AddComponent(typeof(RepeaterRowCollection));
		List<RepeaterRow> rows = new List<RepeaterRow>();
		
		//SELECT BusinessName, ReceiptDate, ItemName, ItemAmount from AllTransactions order by ReceiptDate, BusinessName, ItemName;
		for (int z=0;z<this.webJSONPMName.Count;z++) {
			SimpleJSON.JSONNode node = this.webJSONPMName[z];
			// ReceiptDate - 3
			// BusineessName - 6
			// ItemName - 7
			// ItemAmount - 8
			
			List<RepeaterItemValue> items = new List<RepeaterItemValue>();
			RepeaterItemValue value1 = new RepeaterItemValue() {
				PropertyName = "ItemName", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[1].Value
			};
			RepeaterItemValue value2 = new RepeaterItemValue() {
				PropertyName = "LowestBusiness", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[3].Value
			};
			RepeaterItemValue value3 = new RepeaterItemValue() {
				PropertyName = "LowestPrice", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[4].Value
			};
			RepeaterItemValue value4 = new RepeaterItemValue() {
				PropertyName = "HighestBusiness", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[5].Value
			};
			RepeaterItemValue value5 = new RepeaterItemValue() {
				PropertyName = "HighestPrice", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[6].Value
			};
			RepeaterItemValue value6 = new RepeaterItemValue() {
				PropertyName = "AveragePrice", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[7].Value
			};
			RepeaterItemValue value7 = new RepeaterItemValue() {
				PropertyName = "TotalSpent", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[8].Value
			};
			items.AddRange(new RepeaterItemValue[] {value1,value2,value3,value4,value5,value6,value7});
			RepeaterRow row = new RepeaterRow();
			row.ItemValues = items.ToArray();
			rows.Add(row);
		}
		
		this.PMNameDataValues.Rows = rows.ToArray();
		this.PMNameRepeater.Rows = this.PMNameDataValues;
		this.PMNameRepeater.UpdateRepeater();
	}

	void LoadPMByNameCatReport(int categoryId) {
		if (this.PMNameCatDataValues != null) {
			this.PMNameCatRepeater.KillPreviousItems();
			Destroy(this.PMNameCatDataValues);
		}

		this.PMNameCatDataValues = (RepeaterRowCollection)this.gameObject.AddComponent(typeof(RepeaterRowCollection));
		List<RepeaterRow> rows = new List<RepeaterRow>();
		
		//SELECT BusinessName, ReceiptDate, ItemName, ItemAmount from AllTransactions order by ReceiptDate, BusinessName, ItemName;
		for (int z=0;z<this.webJSONPMNameCat.Count;z++) {
			SimpleJSON.JSONNode node = this.webJSONPMNameCat[z];
			// ReceiptDate - 3
			// BusineessName - 6
			// ItemName - 7
			// ItemAmount - 8

			if (node.Count > 1)
			{

				List<RepeaterItemValue> items = new List<RepeaterItemValue>();
				RepeaterItemValue value1 = new RepeaterItemValue() {
					PropertyName = "ItemName", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[4].Value
				};
				RepeaterItemValue value2 = new RepeaterItemValue() {
					PropertyName = "LowestBusiness", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[6].Value
				};
				RepeaterItemValue value3 = new RepeaterItemValue() {
					PropertyName = "LowestPrice", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[7].Value
				};
				RepeaterItemValue value4 = new RepeaterItemValue() {
					PropertyName = "HighestBusiness", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[8].Value
				};
				RepeaterItemValue value5 = new RepeaterItemValue() {
					PropertyName = "HighestPrice", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[9].Value
				};
				RepeaterItemValue value6 = new RepeaterItemValue() {
					PropertyName = "AveragePrice", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[10].Value
				};
				RepeaterItemValue value7 = new RepeaterItemValue() {
					PropertyName = "TotalSpent", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[11].Value
				};
				items.AddRange(new RepeaterItemValue[] {value1,value2,value3,value4,value5,value6,value7});
				RepeaterRow row = new RepeaterRow();
				row.ItemValues = items.ToArray();
				if (categoryId == 0 || System.Convert.ToInt32(node[1].Value) == categoryId)
					rows.Add(row);
			}
		}
		
		this.PMNameCatDataValues.Rows = rows.ToArray();
		this.PMNameCatRepeater.Rows = this.PMNameCatDataValues;
		this.PMNameCatRepeater.UpdateRepeater();
	}

	void LoadWeeklySpendingReport(int categoryId) {
		if (this.WeeklyDataValues != null) {
			this.WeeklyRepeater.KillPreviousItems();
			Destroy(this.WeeklyDataValues);
		}

		this.WeeklyDataValues = (RepeaterRowCollection)this.gameObject.AddComponent(typeof(RepeaterRowCollection));
		List<RepeaterRow> rows = new List<RepeaterRow>();
		
		//SELECT BusinessName, ReceiptDate, ItemName, ItemAmount from AllTransactions order by ReceiptDate, BusinessName, ItemName;
		for (int z=0;z<this.webJSONWeekly.Count;z++) {
			SimpleJSON.JSONNode node = this.webJSONWeekly[z];
			// ReceiptDate - 3
			// BusineessName - 6
			// ItemName - 7
			// ItemAmount - 8
			
			if (node.Count > 1)
			{
				
				List<RepeaterItemValue> items = new List<RepeaterItemValue>();
				RepeaterItemValue value1 = new RepeaterItemValue() {
					PropertyName = "CategoryName", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[3].Value
				};
				RepeaterItemValue value2 = new RepeaterItemValue() {
					PropertyName = "WeekNumber", PropertyType = RepeaterItemValue.PropertyTypes.Int, PropertyValue = node[5].Value
				};
				RepeaterItemValue value3 = new RepeaterItemValue() {
					PropertyName = "YearNumber", PropertyType = RepeaterItemValue.PropertyTypes.Int, PropertyValue = node[6].Value
				};
				RepeaterItemValue value4 = new RepeaterItemValue() {
					PropertyName = "Starting", PropertyType = RepeaterItemValue.PropertyTypes.DateTime, PropertyValue = node[7].Value
				};
				RepeaterItemValue value5 = new RepeaterItemValue() {
					PropertyName = "Ending", PropertyType = RepeaterItemValue.PropertyTypes.DateTime, PropertyValue = node[8].Value
				};
				RepeaterItemValue value6 = new RepeaterItemValue() {
					PropertyName = "TotalSpent", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[9].Value
				};
				items.AddRange(new RepeaterItemValue[] {value1,value2,value3,value4,value5,value6});
				RepeaterRow row = new RepeaterRow();
				row.ItemValues = items.ToArray();
				if (categoryId == 0 || System.Convert.ToInt32(node[2].Value) == categoryId)
					rows.Add(row);
			}
		}
		
		this.WeeklyDataValues.Rows = rows.ToArray();
		this.WeeklyRepeater.Rows = this.WeeklyDataValues;
		this.WeeklyRepeater.UpdateRepeater();
	}

	void LoadMonthlySpendingReport(int categoryId) {
		if (this.MonthlyDataValues != null) {
			this.MonthlyRepeater.KillPreviousItems();
			Destroy(this.MonthlyDataValues);
		}

		this.MonthlyDataValues = (RepeaterRowCollection)this.gameObject.AddComponent(typeof(RepeaterRowCollection));
		List<RepeaterRow> rows = new List<RepeaterRow>();
		
		//SELECT BusinessName, ReceiptDate, ItemName, ItemAmount from AllTransactions order by ReceiptDate, BusinessName, ItemName;
		for (int z=0;z<this.webJSONMonthly.Count;z++) {
			SimpleJSON.JSONNode node = this.webJSONMonthly[z];
			// ReceiptDate - 3
			// BusineessName - 6
			// ItemName - 7
			// ItemAmount - 8
			
			if (node.Count > 1)
			{
				
				List<RepeaterItemValue> items = new List<RepeaterItemValue>();
				RepeaterItemValue value1 = new RepeaterItemValue() {
					PropertyName = "CategoryName", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[3].Value
				};
				RepeaterItemValue value2 = new RepeaterItemValue() {
					PropertyName = "MonthNumber", PropertyType = RepeaterItemValue.PropertyTypes.Int, PropertyValue = node[5].Value
				};
				RepeaterItemValue value3 = new RepeaterItemValue() {
					PropertyName = "YearNumber", PropertyType = RepeaterItemValue.PropertyTypes.Int, PropertyValue = node[6].Value
				};
				RepeaterItemValue value4 = new RepeaterItemValue() {
					PropertyName = "Starting", PropertyType = RepeaterItemValue.PropertyTypes.DateTime, PropertyValue = node[7].Value
				};
				RepeaterItemValue value5 = new RepeaterItemValue() {
					PropertyName = "Ending", PropertyType = RepeaterItemValue.PropertyTypes.DateTime, PropertyValue = node[8].Value
				};
				RepeaterItemValue value6 = new RepeaterItemValue() {
					PropertyName = "TotalSpent", PropertyType = RepeaterItemValue.PropertyTypes.Float, PropertyValue = node[9].Value
				};
				items.AddRange(new RepeaterItemValue[] {value1,value2,value3,value4,value5,value6});
				RepeaterRow row = new RepeaterRow();
				row.ItemValues = items.ToArray();
				if (categoryId == 0 || System.Convert.ToInt32(node[2].Value) == categoryId)
					rows.Add(row);
			}
		}
		
		this.MonthlyDataValues.Rows = rows.ToArray();
		this.MonthlyRepeater.Rows = this.MonthlyDataValues;
		this.MonthlyRepeater.UpdateRepeater();
	}

	void LoadCategories() {
		this.CategoryBox.DataValues = (RepeaterRowCollection)this.CategoryBox.gameObject.AddComponent(typeof(RepeaterRowCollection));
		List<RepeaterRow> rows = new List<RepeaterRow>();

		for(int z=0;z<this.webJSONCategories.Count;z++) {
			SimpleJSON.JSONNode node = this.webJSONCategories[z];

			if (node.Count > 1)
			{
				
				List<RepeaterItemValue> items = new List<RepeaterItemValue>();
				RepeaterItemValue value1 = new RepeaterItemValue() {
					PropertyName = "CategoryId", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[1].Value
				};
				RepeaterItemValue value2 = new RepeaterItemValue() {
					PropertyName = "CategoryName", PropertyType = RepeaterItemValue.PropertyTypes.String, PropertyValue = node[2].Value
				};
				RepeaterItemValue value3 = new RepeaterItemValue() {
					PropertyName = "ParentCategory", PropertyType = RepeaterItemValue.PropertyTypes.Int, PropertyValue = node[3].Value
				};
				RepeaterItemValue value4 = new RepeaterItemValue() {
					PropertyName = "UserId", PropertyType = RepeaterItemValue.PropertyTypes.Int, PropertyValue = node[4].Value
				};
				items.AddRange(new RepeaterItemValue[] {value1,value2,value3,value4});
				RepeaterRow row = new RepeaterRow();
				row.ItemValues = items.ToArray();
				rows.Add(row);
			}
		}

		this.CategoryBox.DataValues.Rows = rows.ToArray();
		this.CategoryBox.BuildList();
		//this.CategoryBox.ModalDropDown.DataValues = this.CategoryBox.DataValues;
		this.CategoryBox.SetItemText();
		//this.CategoryBox.ModalDropDown.BuildList();
		//this.CategoryBox.ModalDropDown.gameObject.SetActive(false);


		//this.CategoryBox.TriggerSelectionChanged();
	}


	// Update is called once per frame
	void Update () {

		/*
		// Categories Dropdown
		if (webDownloadCategories.isDone && !this.categoriesWebDone) {
			this.categoriesWebDone = true;
			this.webStringCategories = webDownloadCategories.text;

			this.webJSONCategories = SimpleJSON.JSONData.Parse(this.webStringCategories);
			LoadCategories();
		}

		// All Transactions Report
		if (webDownloadAllTransactions.isDone && !this.allTransactionsWebDone) {
			this.allTransactionsWebDone = true;
			this.webStringAllTransactions = webDownloadAllTransactions.text;
			
			this.webJSONAllTransactions = SimpleJSON.JSONData.Parse(this.webStringAllTransactions);
			LoadAllTransactionsReport();
		}

		// Price Matrix by Name
		if (webDownloadPMName.isDone && !this.pmNameWebDone) {
			this.pmNameWebDone = true;
			this.webStringPMName = webDownloadPMName.text;
			
			this.webJSONPMName = SimpleJSON.JSONData.Parse(this.webStringPMName);
			LoadPMByNameReport();
		}

		// Price Matrix by Name/Category
		if (webDownloadPMNameCat.isDone && !this.pmNameCatWebDone) {
			this.pmNameCatWebDone = true;
			this.webStringPMNameCat = webDownloadPMNameCat.text;
			
			this.webJSONPMNameCat = SimpleJSON.JSONData.Parse(this.webStringPMNameCat);
			LoadPMByNameCatReport(1);
		}

		// Weekly by Category
		if (webDownloadWeekly.isDone && !this.weeklyWebDone) {
			this.weeklyWebDone = true;
			this.webStringWeekly = webDownloadWeekly.text;
			
			this.webJSONWeekly = SimpleJSON.JSONData.Parse(this.webStringWeekly);
			LoadWeeklySpendingReport(1);
		}

		// Monthly by Category
		if (webDownloadMonthly.isDone && !this.monthlyWebDone) {
			this.monthlyWebDone = true;
			this.webStringMonthly = webDownloadMonthly.text;
			
			this.webJSONMonthly = SimpleJSON.JSONData.Parse(this.webStringMonthly);
			LoadMonthlySpendingReport(1);
		}
		*/


		if (SecondsBeforeMeltdown > 0f) {
			SecondsBeforeMeltdown -= Time.deltaTime;
		}
		else {


			foreach(LinearChartValue value in barChart.ChartData.ChartValues) {
				value.DataValue = Random.Range(0f,60f);
			}
			barChart.DrawChart();


			foreach(LinearChartValue value in areaChart.ChartData.ChartValues) {
				value.DataValue = Random.Range(0f,60f);
			}
			areaChart.DrawChart();

			foreach(PieChartValue value in pieChart.PieChartValues.PieChartValues) {
				value.DataValue = Random.Range(10f,60f);
			}
			pieChart.PieChartValues.CalculateValues();
			pieChart.DrawPie();



			SecondsBeforeMeltdown = 3f;
			if (!triggered) {
				triggered = true;
			}

		}
		if (VirtualUIFunctions.IsInTouchState && !VirtualUIFunctions.IsInSecondFingerTouchState && !VirtualUIFunctions.ClickUp) {

			if (VirtualUIFunctions.IsModal) {
				return;
			}

			if (VirtualUIFunctions.FirstFingerObjectTag != "") return;

			//Debug.Log("HERE");
			float xCenter = Screen.width / 2f;
			float yCenter = Screen.height / 2f;
			
			float xDistance = VirtualUIFunctions.FirstFingerCurrentPosition.x - xCenter;
			float yDistance = VirtualUIFunctions.FirstFingerCurrentPosition.y - yCenter;
			
			Quaternion Bueler = Camera.main.transform.rotation;
			//Vector3 Bueler2 = new Vector3(0f,0f,0f);
			//Bueler2.x = yDistance * 0.25f;
			//Bueler2.y = -1f * xDistance * 0.25f;
			
			
			Quaternion Bueler2 = Quaternion.Euler(-yDistance * 0.25f, xDistance * 0.25f, 0f);
			if (Bueler2.x > 0.5f) Bueler2.x = 0.5f;
			if (Bueler2.y > 0.5f) Bueler2.y = 0.5f;
			if (Bueler2.z > 0.5f) Bueler2.z = 0.5f;
			if (Bueler2.x < -0.5f) Bueler2.x = -0.5f;
			if (Bueler2.y < -0.5f) Bueler2.y = -0.5f;
			if (Bueler2.z < -0.5f) Bueler2.z = -0.5f;
			
			Camera.main.transform.localEulerAngles = Quaternion.Lerp(Bueler,Bueler2,Time.deltaTime * 2f).eulerAngles;
		}
		else if (VirtualUIFunctions.IsInSecondFingerTouchState && !VirtualUIFunctions.ClickUp && !VirtualUIFunctions.ClickUpSecond) {
			float xDistanceStart = VirtualUIFunctions.SecondFingerStartPosition.x - VirtualUIFunctions.FirstFingerStartPosition.x;
			float yDistanceStart = VirtualUIFunctions.SecondFingerStartPosition.y - VirtualUIFunctions.FirstFingerStartPosition.y;
			float xDistanceCurrent = VirtualUIFunctions.SecondFingerCurrentPosition.x - VirtualUIFunctions.FirstFingerCurrentPosition.x;
			float yDistanceCurrent = VirtualUIFunctions.SecondFingerCurrentPosition.y - VirtualUIFunctions.FirstFingerCurrentPosition.y;
			float distanceStart = Mathf.Sqrt(xDistanceStart * xDistanceStart + yDistanceStart * yDistanceStart);
			float distanceCurrent = Mathf.Sqrt(xDistanceCurrent * xDistanceCurrent + yDistanceCurrent * yDistanceCurrent);
			float deltaDistance = (distanceCurrent - distanceStart);

			Camera.main.fieldOfView -= (deltaDistance / 100.0f);
		}

		if (!VirtualUIFunctions.IsInTextBox) {
			if (Input.GetKey(KeyCode.W)){
				Camera.main.transform.Translate(Vector3.forward * 1f);
			}
			if (Input.GetKey(KeyCode.A)){
				Camera.main.transform.Translate(Vector3.left * 1f);
			}
			if (Input.GetKey(KeyCode.S)){
				Camera.main.transform.Translate(Vector3.back * 1f);
			}
			if (Input.GetKey(KeyCode.D)){
				Camera.main.transform.Translate(Vector3.right * 1f);
			}
			if (Input.GetKey(KeyCode.Q)){
				Camera.main.transform.Translate(Vector3.up * 1f);
			}
			if (Input.GetKey(KeyCode.E)){
				Camera.main.transform.Translate(Vector3.down * 1f);
			}
		}

	}
}

public class TransactionReceipt {
	public long ReceiptId;
	public long? UserId;
	public System.DateTime ReceiptDate;
	public long Business;
}

public class TransactionReceiptSubItem {
	public long SubItemId;
	public long TransactionReceiptId;
	public string ItemName;
	public decimal ItemAmount;
	public int? TransactionCategory;
}

public class Business {
	public int BusinessId;
	public string BusinessName;
}

public class TransactionCategory {
	public int CategoryId;
	public string CategoryName;
	public int? ParentCategory;
}

public class TrackingUser {
	public long UserId;
	public string UserName;
	public string FirstName;
	public string MiddleName;
	public string LastName;
}

public class TestResultSet {
	//SELECT BusinessName, ReceiptDate, ItemName, ItemAmount
	public long ID;
	public string BusinessName;
	public System.DateTime ReceiptDate;
	public string ItemName;
	public decimal ItemAmount;
}