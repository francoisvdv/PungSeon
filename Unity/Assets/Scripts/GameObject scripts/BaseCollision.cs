using UnityEngine;
using System.Collections;

public class BaseCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnTriggerEnter (Collider col) {	
		GameObject newBase = (GameObject)Resources.LoadAssetAtPath("Assets/Bases/p1_base.fbx", typeof(GameObject));
		
		GameObject baseObject = (GameObject)Instantiate(newBase, this.gameObject.transform.position, Quaternion.identity);		
		
		baseObject.AddComponent("BaseCollision");
		baseObject.AddComponent("Light");
		baseObject.AddComponent<SphereCollider>().isTrigger = true;		
		baseObject.transform.localScale += new Vector3(4,4,4);		
		baseObject.transform.rotation = this.gameObject.transform.rotation;	
		
		Destroy(this.gameObject);			
	}
}
