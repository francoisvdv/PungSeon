using UnityEngine;
using System.Collections;

//Attach this to a script and the gameObject will be destroyed after Start
public class Suicide : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		Destroy(this.gameObject);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
