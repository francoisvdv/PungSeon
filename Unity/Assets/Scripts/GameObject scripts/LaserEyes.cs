using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserEyes : MonoBehaviour
{
	Transform particlesL;
	Transform particlesR;
	
	public bool FireAlways;
	
	// Use this for initialization
	void Start ()
	{
		particlesL = transform.Search("EyeL").Find("LaserParticles");
		particlesR = transform.Search("EyeR").Find("LaserParticles");
		
		FireAlways = false;
	}

	// Update is called once per frame
	void Update ()
	{
		if(FireAlways)
			SetLasersEnabled(true);
	}
	
	ParticleRenderer GetParticleRenderer(Transform t)
	{
		return t.GetComponent(typeof(ParticleRenderer)) as ParticleRenderer;
	}
	public void SetLasersEnabled(bool enable)
	{
		GetParticleRenderer(particlesL).enabled = enable;
		GetParticleRenderer(particlesR).enabled = enable;
	}
}
