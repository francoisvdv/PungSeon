using UnityEngine;
using System.Collections;

public class BoxCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnTriggerEnter (Collider col) {
    	Destroy(this.gameObject);
	}
}
