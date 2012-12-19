using UnityEngine;
using System;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
	public bool IsControlled = false;
	public bool AlwaysFireLaserBeams = false;
	
	[NonSerialized]
	public int Health;
	
	
	
	Transform laserTarget;
	
	Transform eyesL;
	Transform eyesR;
	
	Transform laserBeamL;
	Transform laserBeamR;
	
	bool hit;
	RaycastHit hitInfo; //updated once per Update(), containing hitInfo about what the laser target has hit
	
	bool firing = false;
	
	// Use this for initialization
	void Start ()
	{	
		laserTarget = transform.Search("LaserTarget");

		eyesL = transform.Search("EyeL");
		eyesR = transform.Search("EyeR");
		
		laserBeamL = eyesL.Find("LaserParticles");
		laserBeamR = eyesR.Find("LaserParticles");
	}
	
	Vector3 calculateCentroid(params Vector3[] centerPoints)
	{
		var centroid = new Vector3(0,0,0);
		var numPoints = centerPoints.Length;
		
		foreach(var v in centerPoints)
		{
			centroid += v;
		}

		centroid /= numPoints;

		return centroid;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//direction is (pointB - pointA).normalized
		Vector3 start = calculateCentroid(eyesL.position, eyesR.position);
		Ray r = new Ray(start, (laserTarget.position - start).normalized);
		hit = Physics.Raycast(r, out hitInfo, Settings.LaserTargetDistance);
		if(hit)
			laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, hitInfo.distance);
		else
			laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, Settings.LaserTargetDistance);
		
		if(AlwaysFireLaserBeams)
			SetLasersEnabled(true);
		
		if(IsControlled)
		{
			HandleFiring();
		}
	}
	
	ParticleRenderer GetParticleRenderer(Transform t)
	{
		return t.GetComponent(typeof(ParticleRenderer)) as ParticleRenderer;
	}
	void SetLasersEnabled(bool enable)
	{
		ParticleRenderer r1 = laserBeamL.GetComponent(typeof(ParticleRenderer)) as ParticleRenderer;
		ParticleRenderer r2 = laserBeamR.GetComponent(typeof(ParticleRenderer)) as ParticleRenderer;
		
		if(r1 != null)
			r1.enabled = enable;
		if(r2 != null)
			r2.enabled = enable;
	}
	
	void HandleFiring()
	{	
		if(Input.GetKeyDown(Settings.Controls.Fire))
		{
			SetLasersEnabled(true);
			firing = true;
		}
		else if(Input.GetKeyUp (Settings.Controls.Fire))
		{
			SetLasersEnabled(false);
			firing = false;
		}
		
		if(!firing || !hit)
			return;
		
		PlayerScript otherPlayer = hitInfo.transform.GetComponent<PlayerScript>();
		if(otherPlayer == null)
			return;
		
		print ("other player hit");
	}
}
