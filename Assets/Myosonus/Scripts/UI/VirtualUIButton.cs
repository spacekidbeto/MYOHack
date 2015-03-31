using UnityEngine;
using System.Collections;

public class VirtualUIButton : MonoBehaviour {

	public TextMesh TextObject;
	public Color DownColor = Color.cyan;
	public Color UpColor = Color.white;
	

	public Sprite UpSprite;
	public Sprite DownSprite;
	public Sprite CheckMark;
	public Sprite RadioSprite;
	public event System.EventHandler ClickUp;
	public event System.EventHandler CheckedChanged;
	public event System.EventHandler RadioChanged;
	public SpriteRenderer spriteRenderer;
	public SpriteRenderer RadioCheckBoxRenderer;
	public BoxCollider BoundingCollider;
	public string DisplayText;
	public string ValueText;
	public int SelectionIndex = 0;

	public bool IsCheckBox = false;
	public bool IsChecked = false;
	public bool IsRadioButton = false;
	public string RadioButtonGroupName = string.Empty;
	public int RadioButtonIndex = 0;

	bool _state = false;
	bool _lastState = false;

	bool _enabled = true;
	public bool Enabled {
		get {
			return _enabled;
		}
		set {
			if (_enabled && !value) {
				this.Disable();
			}
			else if (!_enabled && value) {
				this.Enable();
			}
			_enabled = value;
		}
	}

	bool IsInSpriteBounds(Vector2 positionToCheck) {
		Vector3 minBounds = Camera.main.WorldToScreenPoint(this.spriteRenderer.bounds.min);
		Vector3 maxBounds = Camera.main.WorldToScreenPoint(this.spriteRenderer.bounds.max);
		if (positionToCheck.x >= minBounds.x && positionToCheck.x <= maxBounds.x && positionToCheck.y >= minBounds.y && positionToCheck.y <= maxBounds.y) {
			return true;
		}
		return false;
	}

	bool IsInSpriteBounds(Vector3 positionToCheck) {
		Vector3 minBounds = Camera.main.WorldToScreenPoint(this.spriteRenderer.bounds.min);
		Vector3 maxBounds = Camera.main.WorldToScreenPoint(this.spriteRenderer.bounds.max);
		if (positionToCheck.x >= minBounds.x && positionToCheck.x <= maxBounds.x && positionToCheck.y >= minBounds.y && positionToCheck.y <= maxBounds.y) {
			return true;
		}
		return false;
	}

	void Disable() {
		Color col = this.spriteRenderer.color;
		col.a = 0.5f;
		this.spriteRenderer.color = col;
	}

	void Enable() {
		Color col = this.spriteRenderer.color;
		col.a = 1f;
		this.spriteRenderer.color = col;
	}

	// Use this for initialization
	void Awake () {
		this.spriteRenderer = (SpriteRenderer)this.gameObject.GetComponent(typeof(SpriteRenderer));
		this.BoundingCollider = (BoxCollider)this.gameObject.GetComponent(typeof(BoxCollider));
	}

	public void SetText(string text) {
		if (this.TextObject != null) {
			this.TextObject.text = text;
		}
	}

	public void ClearRadioSprite() {
		this.RadioCheckBoxRenderer.sprite = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (!this.Enabled) {
			return;
		}

		_state = false;

		if (VirtualUIFunctions.FirstFingerObjectTag != "Button") return;
		if (VirtualUIFunctions.FirstFingerObjectHit != this.transform) return;
		
		RaycastHit hit;
		Ray ray, rayStart;
		ray = Camera.main.ScreenPointToRay(VirtualUIFunctions.FirstFingerCurrentPosition);
		Physics.Raycast(ray, out hit);
		if (hit.collider != null) {
			if (hit.collider.tag == "Button") {
				if (hit.collider.transform == this.transform) {
					_state = true;
				}
			}
		}

		if (_state && !_lastState) {
			if (this.DownSprite != null)
			{
				this.spriteRenderer.sprite = this.DownSprite;
			}
			if (this.TextObject != null) {
				float alpha = this.TextObject.color.a;
				Color col = this.DownColor;
				col.a = alpha;
				this.TextObject.color = col;
			}
		}
		else if (!_state && _lastState) {
			if (this.UpSprite != null) {
				this.spriteRenderer.sprite = this.UpSprite;
			}
			if (this.TextObject != null) {
				float alpha = this.TextObject.color.a;
				Color col = this.UpColor;
				col.a = alpha;
				this.TextObject.color = col;
			}
		}

		if (VirtualUIFunctions.ClickUp) {
			if (_state) {
				if (this.UpSprite != null) {
					this.spriteRenderer.sprite = this.UpSprite;
				}
				if (this.TextObject != null) {
					float alpha = this.TextObject.color.a;
					Color col = this.UpColor;
					col.a = alpha;
					this.TextObject.color = col;
				}
				if (this.IsCheckBox) {
					this.IsChecked = !this.IsChecked;
					if (this.IsChecked) {
						this.RadioCheckBoxRenderer.sprite = this.CheckMark;
					}
					else {
						this.RadioCheckBoxRenderer.sprite = null;
					}
					if (this.CheckedChanged != null) {
						this.CheckedChanged(this, new System.EventArgs());
					}
				}
				if (this.IsRadioButton) {
					this.IsChecked = true;
					this.RadioCheckBoxRenderer.sprite = this.RadioSprite;
					if (this.RadioChanged != null) {
						this.RadioChanged(this, new System.EventArgs());
					}
				}
				if (this.ClickUp != null) {
					this.ClickUp(this, new System.EventArgs());
				}
				_state = false;
				_lastState = false;
			}
		}


		_lastState = _state;
	}
}
