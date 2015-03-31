using UnityEngine;
using System.Collections;

public class VirtualUIFunctions : MonoBehaviour {

	public static Vector3 FirstFingerStartPosition;
	public static Vector3 FirstFingerCurrentPosition;
	public static int FirstFingerId;
	public static int SecondFingerId;
	public static int CurrentFingerId;

	public static Vector3 SecondFingerStartPosition;
	public static Vector3 SecondFingerCurrentPosition;

	public static bool AlreadyClicked;
	public static bool AlreadyClickedSecond;

	public static bool ClickDown;
	public static bool ClickUp;
	public static bool Moved;
	public static bool BackButtonPressed;
	public static bool ClickEvent;
	public static bool FirstClick;

	public static bool ClickDownSecond;
	public static bool ClickUpSecond;
	public static bool MovedSecond;
	public static bool ClickEventSecond;
	public static bool FirstClickSecond;

	public static bool LastDidTouchOnce;

	public static bool IsModal;
	public static string ModalName;

	public static Transform FirstFingerObjectHit;
	public static string FirstFingerObjectTag;
	public static RaycastHit FirstFingerObjectRayInfo;

	void OnGUI() {
	}

	public static bool IsInTouchState {
		get {
			if (ClickDown || Moved || ClickUp) {
				return true;
			}
			return false;
		}
	}
	public static bool IsInTextBox;

	public static bool IsInSecondFingerTouchState {
		get {
			if (ClickDownSecond || MovedSecond || ClickUpSecond) {
				return true;
			}
			return false;
		}
	}

	public static Color LerpColor(Color startColor, Color endColor, float time) {
		Color newColor = new Color();
		newColor.r = Mathf.Lerp(startColor.r, endColor.r, time);
		newColor.g = Mathf.Lerp(startColor.g, endColor.g, time);
		newColor.b = Mathf.Lerp(startColor.b, endColor.b, time);
		newColor.a = Mathf.Lerp(startColor.a, endColor.a, time);
		return newColor;
	}

	public static Vector3 WorldCoordinatesMinimum;
	public static Vector3 WorldCoordinatesMaximum;

	// Use this for initialization
	void Awake () {
		WorldCoordinatesMinimum = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, -10f));
		WorldCoordinatesMaximum = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, -10f));

		AlreadyClicked = false;
		ClickDown = false;
		ClickUp = false;
		ClickEvent = false;
		Moved = false;
		BackButtonPressed = false;

		AlreadyClickedSecond = false;
		ClickDownSecond = false;
		ClickUpSecond = false;
		ClickEventSecond = false;
		MovedSecond = false;
	}
	
	// Update is called once per frame
	void Update () {
		ClickDown = false;
		ClickUp = false;
		Moved = false;

		ClickDownSecond = false;
		ClickUpSecond = false;
		MovedSecond = false;

		FirstClick = false;
		FirstClickSecond = false;

		BackButtonPressed = false;
		if (Input.GetKeyUp (KeyCode.Escape)) {
			// Back Button
			BackButtonPressed = true;
		}
		
		if (Input.touchCount > 0) {
			//Debug.Log ("TOUCH");
			LastDidTouchOnce = true;

			Touch letouch = Input.GetTouch(0);
			FirstFingerCurrentPosition = letouch.position;
			CurrentFingerId = letouch.fingerId;
			if (letouch.phase == TouchPhase.Began) {
				ClickEvent = true;
				//Debug.Log("TOUCH BEGAN");
				if (!AlreadyClicked) { 
					ClickDown = true;
					AlreadyClicked = true;
					FirstClick = true;
					FirstFingerStartPosition = FirstFingerCurrentPosition;
					FirstFingerId = letouch.fingerId;

					RaycastHit hit;
					Ray ray;
					ray = Camera.main.ScreenPointToRay(VirtualUIFunctions.FirstFingerCurrentPosition);
					Physics.Raycast(ray, out hit);
					if (hit.collider != null) {
						if (hit.collider.tag == "Button"
						    || hit.collider.tag == "Slider"
						    || hit.collider.tag == "TextBox"){
							
							FirstFingerObjectTag = hit.collider.tag;
							FirstFingerObjectHit = hit.transform;
							FirstFingerObjectRayInfo = hit;
						}
					}
				}
				else { 
					ClickDown = true;
					AlreadyClicked = false;
				}
			}
			else if (letouch.phase == TouchPhase.Stationary) {
				ClickDown = true;
				AlreadyClicked = false;
			}
			else if (letouch.phase == TouchPhase.Moved) {
				Moved = true;
			}
			else if (letouch.phase == TouchPhase.Ended || letouch.phase == TouchPhase.Canceled) {
				//Debug.Log("TOUCH ENDED");
				ClickDown = false;
				AlreadyClicked = false;
				ClickUp = true;
			}

			if (Input.touchCount > 1) {
				Touch secondTouch = Input.GetTouch(1);
				SecondFingerCurrentPosition = secondTouch.position;

				if (secondTouch.phase == TouchPhase.Began) {
					ClickEventSecond = true;
					//Debug.Log("TOUCH BEGAN");
					if (!AlreadyClickedSecond) { 
						FirstClickSecond = true;
						ClickDownSecond = true;
						AlreadyClickedSecond = true;
						SecondFingerStartPosition = SecondFingerCurrentPosition;
						SecondFingerId = secondTouch.fingerId;
					}
					else { 
						ClickDownSecond = true;
						AlreadyClickedSecond = false;
					}
				}
				else if (secondTouch.phase == TouchPhase.Stationary) {
					ClickDownSecond = true;
					AlreadyClickedSecond = false;
				}
				else if (secondTouch.phase == TouchPhase.Moved) {
					MovedSecond = true;
				}
				else if (secondTouch.phase == TouchPhase.Ended || secondTouch.phase == TouchPhase.Canceled) {
					ClickDownSecond = false;
					AlreadyClickedSecond = false;
					ClickUpSecond = false;

					// Let's reset our first finger position
					FirstFingerStartPosition = FirstFingerCurrentPosition;
				}
			}
		}
		else if (Input.GetMouseButtonDown (0)) {
			FirstFingerCurrentPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
			//Debug.Log("MOUSE DOWN");
			ClickEvent = true;
			if (!AlreadyClicked) { 
				FirstClick = true;
				ClickDown = true;
				AlreadyClicked = true;
				FirstFingerStartPosition = FirstFingerCurrentPosition;

				RaycastHit hit;
				Ray ray;
				ray = Camera.main.ScreenPointToRay(VirtualUIFunctions.FirstFingerCurrentPosition);
				Physics.Raycast(ray, out hit);
				if (hit.collider != null) {
					if (hit.collider.tag == "Button"
					    || hit.collider.tag == "Slider"
					    || hit.collider.tag == "TextBox"){
						
						FirstFingerObjectTag = hit.collider.tag;
						FirstFingerObjectHit = hit.transform;
						FirstFingerObjectRayInfo = hit;
					}
				}
			}
			else { 
				ClickDown = true;
				AlreadyClicked = false;
			}
		} else if (Input.GetMouseButtonUp (0)) {
			FirstFingerCurrentPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
			//Debug.Log("MOUSE UP");
			ClickDown = false;
			AlreadyClicked = false;
			ClickUp = true;

			//alreadyClicked = false;
		} else if (Input.GetMouseButton(0)) {
			FirstFingerCurrentPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
			if (FirstFingerCurrentPosition != FirstFingerStartPosition) {
				Moved = true;
			}
			else {
				ClickDown = true;
			}
		}
		else {
			FirstFingerObjectHit = null;
			FirstFingerObjectTag = "";
		}
	}
}
