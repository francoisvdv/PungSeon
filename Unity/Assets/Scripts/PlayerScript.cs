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
		Health = Options.StartingHealth;
		
		laserTarget = transform.Search("LaserTarget");

		eyesL = transform.Search("EyeL");
		eyesR = transform.Search("EyeR");
		
		laserBeamL = eyesL.Find("LaserParticles");
		laserBeamR = eyesR.Find("LaserParticles");
	}

	// Update is called once per frame
	void Update ()
	{
		//direction is (pointB - pointA).normalized
		Vector3 start = calculateCentroid(eyesL.position, eyesR.position);
		Ray r = new Ray(start, (laserTarget.position - start).normalized);
		hit = Physics.Raycast(r, out hitInfo, Options.LaserTargetDistance);
		if(hit)
			laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, hitInfo.distance);
		else
			laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, Options.LaserTargetDistance);
		
		if(AlwaysFireLaserBeams)
			SetLasersEnabled(true);
		
		if(IsControlled)
		{
			HandleFiring();
			HandleFlagPickup();
		}
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
		if(Input.GetKeyDown(Options.Controls.Fire))
		{
			SetLasersEnabled(true);
			firing = true;
		}
		else if(Input.GetKeyUp (Options.Controls.Fire))
		{
			SetLasersEnabled(false);
			firing = false;
		}
		
		if(!firing || !hit)
			return;
		
		Fire();
	}
	void HandleFlagPickup()
	{
		PickUpFlag();
	}
	void HandleFlagDelivery()
	{
		DeliverFlag();
	}
	void HandleBlockEffectApplication()
	{
		ApplyBlockEffect(0);
	}
	
	void Die()
	{
		GameObject.Destroy(this.gameObject);
	}
	
	void Fire()
	{
		PlayerScript otherPlayer = hitInfo.transform.GetComponent<PlayerScript>();
		if(otherPlayer == null)
			return;
		
		//apparantly there's a hit
		
		ParticleSystem ps = laserTarget.GetComponent<ParticleSystem>();
		//if(!ps.isPlaying)
			ps.Play();
		
		otherPlayer.UpdateHealth(otherPlayer.Health - 1);
	}
	public void UpdateHealth(int newAmount)
	{
		Health = newAmount;
		
		if(Health <= 0)
			Die();
	}
	void PickUpFlag()	
	{
	}
	void DeliverFlag()
	{
	}
	void ApplyBlockEffect(int id)
	{
	}
}