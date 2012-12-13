using UnityEngine;
using System.Collections;

public class FollowLight : MonoBehaviour {
	
	public GameObject Target;
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = new Vector3(Target.transform.position.x, Target.transform.position.y + 2, Target.transform.position.z);
	}
}