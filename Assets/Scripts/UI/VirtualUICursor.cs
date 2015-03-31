using UnityEngine;
using System.Collections;

public class VirtualUICursor : MonoBehaviour {

	public float StartTime = 1f;
	bool CursorOn = true;
	Color rendererColor;
	bool paused = false;

	float CurrentTime = 1f;
	// Use this for initialization
	void Start () {
		rendererColor = this.GetComponent<Renderer>().material.color;
		CurrentTime = StartTime;
		StartCoroutine("Blink");
	}

	public void Pause() {
		paused = true;
	}

	public void Resume() {
		paused = false;
	}

	IEnumerator Blink() {
		while (true) {
			while (CurrentTime > 0f) {
					yield return new WaitForSeconds (0.25f);
			}
			if (!paused) {
					Color col = rendererColor;
					if (!CursorOn) {
							col.a *= 0.25f;
					}
					this.GetComponent<Renderer>().material.color = col;
			}
			CursorOn = !CursorOn;
			CurrentTime = StartTime;
		}
	}

	// Update is called once per frame
	void Update () {
		CurrentTime -= Time.deltaTime;
	}
}
