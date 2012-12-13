using UnityEngine;
using System.Collections;

// Require the following components when using this script
[RequireComponent(typeof(AudioSource))]
public class LaserEyes : MonoBehaviour
{
	public LineRenderer laserPrefab; 	// public variable for Laser prefab 
	public Transform LaserTarget;

	private Transform EyeL;				// Left Eye position transform
	private Transform EyeR;				// Right Eye position transform
	private LineRenderer laserL;		// Left Eye Laser Line Renderer
	private LineRenderer laserR;		// Right Eye Laser Line Renderer
	private bool shot;					// a toggle for when we have shot the laser

	
	void Start()
	{		
		// creating the two line renderers to initialise our variables
		laserL = new LineRenderer();
		laserR = new LineRenderer();
		
		// initialising eye positions
		EyeL = transform.Find("EyeL");
		EyeR = transform.Find("EyeR");

		// setting up the audio component
		audio.loop = true;
		audio.playOnAwake = false;
	}
	
	
	void Update ()
	{
		// if the look weight has been increased to 0.9, and we have not yet shot..
		if(Input.GetKeyDown(KeyCode.V) && !shot)
		{
			// instantiate our two lasers
			laserL = Instantiate(laserPrefab) as LineRenderer;
			laserR = Instantiate(laserPrefab) as LineRenderer;
			
			// register that we have shot once
			shot = true;
			// play the laser beam effect
			audio.Play ();
		}
		// if the look weight returns to normal
		else if(Input.GetKeyUp (KeyCode.V) && shot)
		{
			Destroy(laserL.gameObject);
			Destroy(laserR.gameObject);
			
			// Destroy the laser objects
			//Destroy(laserL.gameObject);
			//Destroy(laserR.gameObject);
			
			// reset the shot toggle
			shot = false;
			// stop audio playback
			audio.Stop();
		}
		
		// if our laser line renderer objects exist..
		if(laserL != null && laserR != null)
		{
			// set positions for our line renderer objects to start at the eyes and end at the enemy position, registered in the bot control script
			laserL.SetPosition(0, EyeL.position);
			laserL.SetPosition(1, LaserTarget.position);
			laserR.SetPosition(0, EyeR.position);
			laserR.SetPosition(1, LaserTarget.position);
		}
	}
}
