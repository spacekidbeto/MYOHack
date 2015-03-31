using UnityEngine;
using System.Collections;

// Credit to "save" on http://answers.unity3d.com/questions/138373/editable-3d-text.html
public class VirtualUITextBox : MonoBehaviour {

	public TextMesh TextObject;
	public BoxCollider TextBoxCollider;
	public string GUIFieldName;
	public int MaxLength = 80;
	public string NullPlaceHolder = "\\Empty/";
	public Transform Cursor;
	public Transform SelectionCursor;
	public MeshFilter CursorSelectionFilter;
	public MeshRenderer CursorSelectionRenderer;
	public Color SelectionColor;
	public Color CursorColor;
	public VirtualUICursor CursorScript;

	//public TextMesh DebugText;

	//public LineRenderer LineObject;

	public bool InEditMode = false;
	string StoredString;
	string GuiString;
	bool firstClick = false;
	// Use this for initialization
	void Start () {

		this.SelectionColor = this.SelectionCursor.GetComponent<Renderer>().material.color;
		this.CursorColor = this.Cursor.GetComponent<Renderer>().material.color;
		Font fnt = this.TextObject.font;

		float halfSizeY = this.TextObject.GetComponent<Renderer>().bounds.size.y / 2f;
		float fullSizeX = this.TextObject.GetComponent<Renderer>().bounds.size.x;
		Vector3 min = new Vector3(0f,0f,0f);
		Vector3 max = new Vector3(fullSizeX,0f,0f);

		StoredString = this.TextObject.text;
		GuiString = StoredString;

		// Visual Aid for Focus
		this.SetTextBlurred();

		CheckChars();
		FitCollider();

		this.HideSelectionCursor();
		//this.HideCursor();
	}

	void SetTextFocused() {
		Color col = this.TextObject.color;
		col.a = 1f;
		this.TextObject.color = col;
	}

	void SetTextBlurred() {
		Color col = this.TextObject.color;
		col.a = 0.5f;
		this.TextObject.color = col;
	}

	// Update is called once per frame
	void Update () {
		if (VirtualUIFunctions.IsInTouchState) {
			if (VirtualUIFunctions.FirstFingerObjectTag == "TextBox") {
				if (VirtualUIFunctions.FirstFingerObjectHit == this.TextBoxCollider.transform) {
					this.InEditMode = true;
					VirtualUIFunctions.IsInTextBox = true;
					this.ShowCursor();
					this.SetTextFocused();
					this.SetCursorPosition();
				}
				else {
					this.InEditMode = false;
					this.HideSelectionCursor();
					//this.HideCursor();
					VirtualUIFunctions.IsInTextBox = false;
					this.SetTextBlurred();
					CheckChars();
				}
			}
			else {
				float boxHeight = Screen.height - 100f;
				if (VirtualUIFunctions.FirstFingerCurrentPosition.y < boxHeight) {
					this.InEditMode = false;
					this.HideSelectionCursor();
					//this.HideCursor();
					VirtualUIFunctions.IsInTextBox = false;
					this.SetTextBlurred();
					CheckChars();
				}
			}
		}
	}

	void PauseCursor() {
		this.CursorScript.Pause();
	}

	void ResumeCursor() {
		this.CursorScript.Resume();
	}

	void HideSelectionCursor() {
		if (this.SelectionColor.a != 0f) {
			Color col = this.SelectionColor;
			col.a = 0f;
			this.SelectionCursor.GetComponent<Renderer>().material.color = col;
		}
	}

	void ShowSelectionCursor() {
		Color col = this.SelectionColor;
		this.SelectionCursor.GetComponent<Renderer>().material.color = col;
	}

	void HideCursor() {
		if (this.CursorColor.a != 0f) {
			Color col = this.CursorColor;
			col.a = 0f;
			this.Cursor.GetComponent<Renderer>().material.color = col;
			this.PauseCursor();
		}
	}

	void ShowCursor() {
		Color col = this.CursorColor;
		this.Cursor.GetComponent<Renderer>().material.color = col;
		this.ResumeCursor();
	}

	void SetCursorPosition() {
		TextAlignment alignment = TextAlignment.Center;
		Vector3 boundsPoint = TextBoxCollider.bounds.center;
		if (TextObject.anchor == TextAnchor.LowerLeft || TextObject.anchor == TextAnchor.MiddleLeft || TextObject.anchor == TextAnchor.UpperLeft) {
			alignment = TextAlignment.Left;
			boundsPoint = TextBoxCollider.bounds.min;
		}
		else if (TextObject.anchor == TextAnchor.LowerRight || TextObject.anchor == TextAnchor.MiddleRight || TextObject.anchor == TextAnchor.UpperRight) {
			alignment = TextAlignment.Right;
			boundsPoint = TextBoxCollider.bounds.max;
		}
		boundsPoint.y = TextBoxCollider.bounds.max.y;

		TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
		//te.SelectNone ();
		bool selectNone = false;
		if (VirtualUIFunctions.FirstClick) {
			//Debug.Log("SELECT NONE");
			//this.DebugText.text = "SELECT NONE " + System.DateTime.Now;
			te.SelectNone();
			selectNone = true;
			//te.selectPos = character + 1;
		}

		float totalWidth = 0f;
		float positionWidth = 0f;
		int character = 0;
		foreach(char ch in GuiString.ToCharArray()) {
			CharacterInfo info;
			bool success = TextObject.font.GetCharacterInfo(ch, out info, this.TextObject.fontSize);
			totalWidth += info.width;
		}
		foreach(char ch in GuiString.ToCharArray()) {
			CharacterInfo info;
			bool success = TextObject.font.GetCharacterInfo(ch, out info, this.TextObject.fontSize);
			float lastWidth = positionWidth;
			positionWidth += (info.width);
			float halfWidth = lastWidth + ((positionWidth - lastWidth) / 2f);

			//Debug.Log(lastWidth + " / " + halfWidth + " / " + positionWidth);

			float nextWidthPercent = positionWidth / totalWidth;
			float nextCursorPosition = TextBoxCollider.size.x * nextWidthPercent;

			float middleWidthPercent = halfWidth / totalWidth;
			float middleCursorPosition = TextBoxCollider.size.x * middleWidthPercent;

			float lastWidthPercent = lastWidth / totalWidth;
			float lastCursorPosition = TextBoxCollider.size.x * lastWidthPercent;

			float currentFingerX = VirtualUIFunctions.FirstFingerCurrentPosition.x;

			Vector3 nextPos = this.Cursor.localPosition;
			Vector3 middlePos = this.Cursor.localPosition;
			Vector3 lastPos = this.Cursor.localPosition;

			if (alignment == TextAlignment.Right) {
				nextPos.x = TextBoxCollider.size.x;
				middlePos.x = TextBoxCollider.size.x;
				lastPos.x = TextBoxCollider.size.x;
			}
			else if (alignment == TextAlignment.Center) {
				float leftPos = Vector3.zero.x - (TextBoxCollider.size.x / 2f);
				nextPos.x = leftPos + nextCursorPosition;
				middlePos.x = leftPos + middleCursorPosition;
				lastPos.x = leftPos + lastCursorPosition;

				//Debug.Log(nextPos + " / " + middlePos + " / " + lastPos);
			}
			else if (alignment == TextAlignment.Left) {
				nextPos.x = nextCursorPosition;
				middlePos.x = middleCursorPosition;
				lastPos.x = lastCursorPosition;
			}

			Vector3 cursorLastPosition = this.Cursor.localPosition;
			this.Cursor.localPosition = nextPos;
			Vector3 nextPosWorld = this.Cursor.position;
			this.Cursor.localPosition = middlePos;
			Vector3 middlePosWorld = this.Cursor.position;
			this.Cursor.localPosition = lastPos;
			Vector3 lastPosWorld = this.Cursor.position;
			this.Cursor.localPosition = cursorLastPosition;



			//Vector3 point = VirtualUIFunctions.FirstFingerObjectRayInfo.point;
			float nextNewX = Camera.main.WorldToScreenPoint(nextPosWorld).x;
			float middleNewX = Camera.main.WorldToScreenPoint(middlePosWorld).x;
			float lastNewX = Camera.main.WorldToScreenPoint(lastPosWorld).x;
			//float screenPoint = Camera.main.WorldToScreenPoint(point).x;
			//Debug.Log(character + " / " +  currentFingerX + " / " + nextNewX + " / " + middleNewX + " / " + lastNewX);

			if (currentFingerX >= lastNewX && currentFingerX < middleNewX) {
				te.pos = character;
				return;
			}
			else if (currentFingerX >= middleNewX && currentFingerX < nextNewX) {
				te.pos = character + 1;
				return;
			}

			/*
			if (lastNewX > nextNewX && nextPos.x > lastPos.x) {
				if (currentFingerX >= nextNewX && currentFingerX < middleNewX) {
					te.pos = character;
					return;
				}
				else if (currentFingerX >= middleNewX && currentFingerX < lastNewX) {
					te.pos = character - 1;
					return;
				}
			}
			else {
				if (currentFingerX >= lastNewX && currentFingerX < middleNewX) {
					te.pos = character - 1;
					return;
				}
				else if (currentFingerX >= middleNewX && currentFingerX < nextNewX) {
					te.pos = character;
					return;
				}
			}
			*/
			character++;
		}
	}

	void UpdateCursorAndLocation() {
		TextAlignment alignment = TextAlignment.Center;
		Vector3 boundsPoint = TextBoxCollider.bounds.center;
		if (TextObject.anchor == TextAnchor.LowerLeft || TextObject.anchor == TextAnchor.MiddleLeft || TextObject.anchor == TextAnchor.UpperLeft) {
			alignment = TextAlignment.Left;
			boundsPoint = TextBoxCollider.bounds.min;
		}
		else if (TextObject.anchor == TextAnchor.LowerRight || TextObject.anchor == TextAnchor.MiddleRight || TextObject.anchor == TextAnchor.UpperRight) {
			alignment = TextAlignment.Right;
			boundsPoint = TextBoxCollider.bounds.max;
		}
		boundsPoint.y = TextBoxCollider.bounds.max.y;
		Vector3 xPoint = Camera.main.WorldToScreenPoint(boundsPoint);

		// Thanks to http://answers.unity3d.com/questins/22857/moving-the-keyboard-cursor-position-to-the-end-of.html
		TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
		//te.SelectNone();
		
		
		int pos = te.pos;
		int selPos = te.selectPos;
		float selStart = 0f;
		float selEnd = 0f;
		
		int selectionStart = 0;
		int selectionEnd = 0;
		if (pos > selPos) {
			selectionStart = selPos;
			selectionEnd = pos;
		}
		else {
			selectionStart = pos;
			selectionEnd = selPos;
		}
		
		float totalWidth = 0f;
		float positionWidth = 0f;
		int character = 0;
		foreach(char ch in GuiString.ToCharArray()) {
			CharacterInfo info;
			bool success = TextObject.font.GetCharacterInfo(ch, out info, this.TextObject.fontSize);
			if (character == selectionStart) {
				selStart = totalWidth;
			} 
			if (character == selectionEnd) {
				selEnd = totalWidth;
			}
			totalWidth += info.width;
			
			if (character < pos) {
				positionWidth += info.width;
			}
			character++;
		}
		
		if (VirtualUIFunctions.FirstClick) {
			te.SelectNone();
		}
		
		if (te.hasSelection)
		{
			ShowSelectionCursor();
			if (selEnd == 0f && selStart > 0f) selEnd = totalWidth;
		}
		else {
			HideSelectionCursor();
		}
		if (te.hasSelection && selStart == 0f && selEnd == 0f) {
			// SelectAll;
			selStart = 0f;
			selEnd = totalWidth;
		}
		
		if (totalWidth > 0f)
		{
			float widthPercent = positionWidth / totalWidth;
			float newCursorPosition = TextBoxCollider.size.x * widthPercent;
			Vector3 cursorPos = this.Cursor.localPosition;
			if (alignment == TextAlignment.Right) {
				cursorPos.x = TextBoxCollider.size.x;
			}
			else if (alignment == TextAlignment.Center) {
				float leftPos = Vector3.zero.x - (TextBoxCollider.size.x / 2f);
				cursorPos.x = leftPos + newCursorPosition;
			}
			else if (alignment == TextAlignment.Left) {
				cursorPos.x = newCursorPosition;
			}
			this.Cursor.localPosition = cursorPos;
			
			
			float widthSelectionPercent = (selEnd - selStart) / totalWidth;
			float widthSelection = (selEnd - selStart);
			float endPercent = (selEnd) / totalWidth;
			float startPercent = (selStart) / totalWidth;
			
			Vector3 locScale = this.SelectionCursor.localScale;
			locScale.x = this.TextBoxCollider.size.x * (widthSelectionPercent);
			this.SelectionCursor.localScale = locScale;
			
			Vector3 localPos = this.SelectionCursor.localPosition;
			localPos.x = 0f;
			if (selPos > pos) {
				localPos = this.Cursor.localPosition;
				localPos.x += widthSelectionPercent * TextBoxCollider.size.x / 2f;
			}
			else {
				localPos.x -= TextBoxCollider.size.x / 2f;
				localPos.x += widthSelectionPercent * TextBoxCollider.size.x / 2f;
				localPos.x += startPercent * TextBoxCollider.size.x;
			}
			
			this.SelectionCursor.localPosition = localPos;
		}
	}

	void OnGUI() {
		if (InEditMode) {

			GUI.SetNextControlName(this.GUIFieldName);
			GUI.FocusControl(this.GUIFieldName);

			TextAlignment alignment = TextAlignment.Center;
			Vector3 boundsPoint = TextBoxCollider.bounds.center;
			if (TextObject.anchor == TextAnchor.LowerLeft || TextObject.anchor == TextAnchor.MiddleLeft || TextObject.anchor == TextAnchor.UpperLeft) {
				alignment = TextAlignment.Left;
				boundsPoint = TextBoxCollider.bounds.min;
			}
			else if (TextObject.anchor == TextAnchor.LowerRight || TextObject.anchor == TextAnchor.MiddleRight || TextObject.anchor == TextAnchor.UpperRight) {
				alignment = TextAlignment.Right;
				boundsPoint = TextBoxCollider.bounds.max;
			}
			boundsPoint.y = TextBoxCollider.bounds.max.y;
			Vector3 xPoint = Camera.main.WorldToScreenPoint(boundsPoint);

			/*
			Vector2 guiX = GUIUtility.ScreenToGUIPoint(new Vector2(xPoint.x, Camera.main.pixelHeight - xPoint.y));
			if (alignment == TextAlignment.Right) { guiX.x -= 200f; }
			else if (alignment == TextAlignment.Center) { guiX.x -= 100f; }
			*/

			float halfScreenWidth = (float)(Screen.width / 2);
			float boxWidth = 200f;

			string lastString = GuiString;
			//GuiString = GUI.TextField(new Rect(guiX.x,guiX.y - 25f,200f,25f), GuiString, MaxLength);
			GuiString = GUI.TextField(new Rect(halfScreenWidth - (boxWidth / 2f),25f,boxWidth,25f), GuiString, MaxLength);
			if (GuiString != lastString) {
				this.TextObject.text = GuiString;
				this.FitCollider();
			}

			// Listen for Keys
			if (Event.current.isKey) {
				this.TextObject.text = GuiString;
				FitCollider();
				this.UpdateCursorAndLocation();
				if (Event.current.keyCode != null) {
					if (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Return) {
						InEditMode = false;
						VirtualUIFunctions.IsInTextBox = false;
						this.SetTextBlurred();    //Alpha 50% on deselection
						CheckChars();    //Check so the 3d-text isn't empty
						this.HideSelectionCursor();
						this.HideCursor();
					}
				}
			}
			else if (VirtualUIFunctions.IsInTouchState) {
				this.UpdateCursorAndLocation();
			}
			

		}

	}

	void CheckChars() {
		if (this.TextObject.text == string.Empty) {
			this.TextObject.text = this.NullPlaceHolder;
			FitCollider();
		}
	}

	void FitCollider() {
		Vector3 size = TextBoxCollider.size;


		size.x = TextObject.GetComponent<Renderer>().bounds.size.x;
		TextBoxCollider.size = size;
		Vector3 center = TextBoxCollider.center;
		if (TextObject.anchor == TextAnchor.LowerCenter || TextObject.anchor == TextAnchor.MiddleCenter || TextObject.anchor == TextAnchor.UpperCenter) {
			center.x = 0f;
		}
		else if (TextObject.anchor == TextAnchor.LowerLeft || TextObject.anchor == TextAnchor.MiddleLeft || TextObject.anchor == TextAnchor.UpperLeft) {
			center.x = size.x / 2f;
		}
		else if (TextObject.anchor == TextAnchor.LowerRight || TextObject.anchor == TextAnchor.MiddleRight || TextObject.anchor == TextAnchor.UpperRight) {
			center.x = 0f - (size.x / 2f);
		}
		TextBoxCollider.center = center;
	}
}
