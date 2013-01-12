using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public GameObject[] blocks;
	public GameObject terrain;
	public GameObject[] bases;
	
	// Use this for initialization
	void Start () {	
		spawnBlocks ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void spawnBlocks()
	{
		for(int i = 0; i < 10; i++)
		{
			spawnBlock(Random.Range(0, blocks.Length));
		}
	}
	
	void spawnBlock(int id)
    {		
		GameObject blockObject = (GameObject)Instantiate(blocks[id], new Vector3(Random.Range(-50, 50), 10, Random.Range(-50, 50)), Quaternion.identity);		
		
		blockObject.AddComponent<BoxCollider>().isTrigger = true;
		blockObject.AddComponent("BoxCollision");
		blockObject.AddComponent("Light");
		
		RaycastHit hit;
	    Physics.Raycast(blockObject.transform.position, -Vector3.up, out hit);
		
		if ( hit.collider.gameObject.Equals(terrain) ){
			var distanceToGround = hit.distance;
			Vector3 dist = new Vector3(0, -distanceToGround+1, 0);
			blockObject.transform.position += dist;
			print("Spawned a block with id: "+id+" distance to ground: "+distanceToGround);
		} else {
			Destroy(blockObject);
			print ("No block spawned, another object was in the way!");
			spawnBlock (id);
		}		
	}	
	
	void spawnAtRandomPosition( GameObject g ){
		
	}
}
