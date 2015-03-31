using UnityEngine;
using System.Collections;

public class VirtualUISliderValueChangedArgs : System.EventArgs {

	public float Percentage;
	public VirtualUISliderValueChangedArgs(float percentage) {
		this.Percentage = percentage;
	}
}

public delegate void VirtualUISliderValueChangedHandler(object sender, VirtualUISliderValueChangedArgs e);