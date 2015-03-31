using UnityEngine;
using System.Collections;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

// Change the material when certain poses are made with the Myo armband.
// Vibrate the Myo armband when a fist pose is made.
public class MyosonusColorBoxByPose : MonoBehaviour
{
	// Myo game object to connect with.
	// This object must have a ThalmicMyo script attached.
	public GameObject myo = null;
	
	// Materials to change to when poses are made.
	public Material waveInMaterial;
	public Material waveOutMaterial;
	public Material doubleTapMaterial;
	
	// The pose from the last update. This is used to determine if the pose has changed
	// so that actions are only performed upon making them rather than every frame during
	// which they are active.
	private Pose _lastPose = Pose.Unknown;
	
	//Sound Scripts
	private float CurrentVol=1.00f;
	private float NoteOffVol=0.00f;
	private int OctaveOffset;
	private int TransposeOffset;
	public AudioClip [] Instruments;
	public AudioClip [] Instruments2;
	GameObject[] Sounds1 = new GameObject[12];
	public GameObject[] Spheres = new GameObject[12];
	GameObject[] Sounds2 = new GameObject[12];
	int[] note = new int[12];
	volatile bool[] playedSound = new bool[12];
	bool[] highlightedState = new bool[12];
	public Transform SoundParent;

	
	//int[] pitches1 = new int[] { 0,2, 3,5, 7,8,10 ,12,14 ,15,17,19 ,20,22 ,24,26,27 ,29,31 ,32,34,36,38,39,41,43,44,46,48,50,51,53,55,56,58,60,62,63,-23, -20, -18, -15, -13, -11, -8, -6, -3, -1, 1, 4, 6,9,11,13,16 ,18,21,23 ,25,28 ,30,33,35 ,37,40 ,42,45,47,49,52,54,57,59,61,64};
	public static int noteselect = 0;

	public SoundInstantiationScript InstiatableSound;
	
	void Start () {
		
		for (int i = 0; i < 12; i++) {
			//Create the game object
			Sounds1 [i] = new GameObject ();  
			Sounds1 [i].gameObject.AddComponent<AudioSource> ();
			//Sounds2 [i] = new GameObject ();  
			//Sounds2 [i].gameObject.AddComponent<AudioSource> ();
			playedSound [i] = false;
			note [i] = 0;
			//int[] pitches1 = new int[] { -24, -22, -21, -19, -17, -16, -14, -12, -10, -9, -7, -5, -4, -2,0,2, 3,5, 7,8,10 ,12,14 ,15,17,19 ,20,22 ,24,26,27 ,29,31 ,32,34,36,38,39,41,43,44,46,48,50,51,53,55,56,58,60,62,63,-23, -20, -18, -15, -13, -11, -8, -6, -3, -1, 1, 4, 6,9,11,13,16 ,18,21,23 ,25,28 ,30,33,35 ,37,40 ,42,45,47,49,52,54,57,59,61,64} ;
			
		}
		for (int z=0; z<12; z++) {
			Sounds1 [z].GetComponent<AudioSource> ().clip = Instruments[z];
			//Sounds2 [z].GetComponent<AudioSource> ().clip = Instruments2[z];
		}

		OctaveOffset=0;
		TransposeOffset=0;
		
	}
	
	void FixedUpdate ()
	{
		
	}
	// Update is called once per frame.
	void Update ()
	{
		
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
		RaycastHit hit = new RaycastHit ();
		
		if (Physics.Raycast (ray, out hit)) { 
			print (hit.collider.gameObject.name);
			
			for (int t = 0; t < 12; t++){
				
				if(hit.collider.gameObject==Spheres[t]) {
					if (!playedSound[t]) {
						SoundInstantiationScript newSound = (SoundInstantiationScript)Instantiate(InstiatableSound);
						newSound.InstantiatedSoundSource = Sounds1[t].GetComponent<AudioSource>();
						newSound.StartSound(Mathf.Pow (2f,(OctaveOffset+TransposeOffset)/12f));
						newSound.transform.parent = SoundParent.transform;
					}
					playedSound[t] = true;
					noteselect=t;
					highlightedState[t]=true;
					hit.collider.gameObject.GetComponent<Renderer>().material = waveInMaterial;
				} else if(highlightedState[t]==true) {
					highlightedState[t]=false;
					playedSound[t] = false;
					Spheres[t].gameObject.GetComponent<Renderer>().material = doubleTapMaterial;
				}
				
			}
			
			
		}
		
		// Access the ThalmicMyo component attached to the Myo game object.
		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();
		
		// Check if the pose has changed since last update.
		// The ThalmicMyo component of a Myo game object has a pose property that is set to the
		// currently detected pose (e.g. Pose.Fist for the user making a fist). If no pose is currently
		// detected, pose will be set to Pose.Rest. If pose detection is unavailable, e.g. because Myo
		// is not on a user's arm, pose will be set to Pose.Unknown.
		if (thalmicMyo.pose != _lastPose) {
			_lastPose = thalmicMyo.pose;
			
			// Vibrate the Myo armband when a fist is made.
			if (thalmicMyo.pose == Pose.Fist) {
				//thalmicMyo.Vibrate (VibrationType.Medium);
				
				int y = 4;
				
				

				ExtendUnlockAndNotifyUserAction (thalmicMyo);
				
				// Change material when wave in, wave out or double tap poses are made.
			} else if (thalmicMyo.pose == Pose.WaveIn) {
				GetComponent<Renderer>().material = waveInMaterial;
				
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			} else if (thalmicMyo.pose == Pose.WaveOut) {
				GetComponent<Renderer>().material = waveOutMaterial;
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			} else if (thalmicMyo.pose == Pose.DoubleTap) {
				GetComponent<Renderer>().material = doubleTapMaterial;
				for (int y = 0; y < 12; y++){
					if(Sounds1[y].GetComponent<AudioSource>().isPlaying){
						Sounds1[y].GetComponent<AudioSource>().Stop();
					}
				}
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			}
		}
	}
	
	// Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was
	// recognized.
	void ExtendUnlockAndNotifyUserAction (ThalmicMyo myo)
	{
		ThalmicHub hub = ThalmicHub.instance;
		
		if (hub.lockingPolicy == LockingPolicy.Standard) {
			myo.Unlock (UnlockType.Timed);
		}
		
		myo.NotifyUserAction ();
	}
}