using UnityEngine;
using System.Collections;

public class SoundInstantiationScript : MonoBehaviour {

	public AudioSource InstantiatedSoundSource;
	bool started = false;
	bool isDying = false;
	public float CountdownTime = 3f;
	float StartingCountdownTime = 3f;
	// Use this for initialization
	void Start () {

	}

	public void StartSound(float pitch) {
		StartingCountdownTime = CountdownTime;
		started = true;
		InstantiatedSoundSource.pitch = pitch;
		InstantiatedSoundSource.Play();
	}

	public void StartDestroy() {
		isDying = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (started && InstantiatedSoundSource.isPlaying && isDying) {
			CountdownTime -= Time.deltaTime;
			if (CountdownTime < 0f) {
				CountdownTime = 0f;
			}
			InstantiatedSoundSource.volume = CountdownTime / StartingCountdownTime;
		}
		else if (started && !InstantiatedSoundSource.isPlaying && isDying) {
			DestroyObject(this.gameObject);
		}
	}
}
