using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
	Transform laserTarget;
	Transform eyesL;
	Transform eyesR;
	
	// Use this for initialization
	void Start ()
	{
		laserTarget = transform.Search("LaserTarget");
		laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, Settings.LaserTargetDistance);
		
		eyesL = transform.Search("EyeL");
		eyesR = transform.Search("EyeR");
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
		RaycastHit hitInfo;
		if(!Physics.Raycast(r, out hitInfo, Settings.LaserTargetDistance))
		{
			laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, Settings.LaserTargetDistance);
			return;
		}
				
		laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, hitInfo.distance);
	}
}
