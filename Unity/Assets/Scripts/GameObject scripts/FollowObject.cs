using UnityEngine;
using System.Collections;

//This script makes a GameObject follow another GameObject on the xyz axis.
public class FollowObject : MonoBehaviour
{
	public bool PosX = true, PosY = true, PosZ = true;
	public Vector3 PosOffset;
	public bool RotX, RotY, RotZ;
	public GameObject Target;
	
	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(PosX || PosY | PosZ)
		{
			float newPosX = PosX ? Target.transform.position.x + PosOffset.x : transform.position.x;
			float newPosY = PosY ? Target.transform.position.y + PosOffset.y : transform.position.y;
			float newPosZ = PosZ ? Target.transform.position.z + PosOffset.z : transform.position.z;
			transform.position = new Vector3(newPosX, newPosY, newPosZ);
		}
		
		if(RotX || RotY || RotZ)
		{
			float newRotX = RotX ? Target.transform.rotation.eulerAngles.x - transform.rotation.eulerAngles.x : 0;
			float newRotY = RotY ? Target.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y : 0;
			float newRotZ = RotZ ? Target.transform.rotation.eulerAngles.z - transform.rotation.eulerAngles.z : 0;
			
			transform.Rotate (newRotX, newRotY, newRotZ);
		}
	}
}