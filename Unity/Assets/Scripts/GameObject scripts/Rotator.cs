using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
    public Vector3 Speed = new Vector3(1, 1, 1);

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(Speed);
	}
}
