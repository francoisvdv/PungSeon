using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int BlockSpawnStartY = 100;
    public int BlockSpawnMinX = 0, BlockSpawnMaxX = 150;
    public int BlockSpawnMinZ = 0, BlockSpawnMaxZ = 150;

	public GameObject[] blockPrefabs;
	public GameObject[] basePrefabs;
    public Material[] baseMaterials;
    public GameObject robotPrefab;
    public Material[] robotMaterials;

    GameObject terrain;
    GameObject[] spawnPoints;

    List<Player> players = new List<Player>();

	// Use this for initialization
	void Start ()
    {
        Instance = this;

        OnLevelWasLoaded(Application.loadedLevel);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnLevelWasLoaded(int level)
    {
        if (level != 1) //TerrainMap game world
            return;
        
        SetUpScene();
    }

    void SetUpScene()
    {
        terrain = GameObject.Find("Terrain");

        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnBlocks();

        foreach (var v in Client.Instance.GetOutgoingAddresses())
        {
            spawnRobot(v);
        }
    }

    void spawnRobot(System.Net.IPAddress ip, GameObject spawnPoint = null)
    {
        Player player = null;
        foreach (Player p in players)
        {
            if (p.PlayerIP.Equals(ip))
            {
                player = p;
                break;
            }
        }

        if (spawnPoint == null)
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length - 1)];

        GameObject robot = null;
        if (player == null)
        {
            robot = (GameObject)Instantiate(robotPrefab);

            player = robot.GetComponent<Player>();
            player.PlayerIP = ip;
            players.Add(player);

            SkinnedMeshRenderer smr = (SkinnedMeshRenderer)robot.GetComponentInChildren(typeof(SkinnedMeshRenderer));
            smr.material = robotMaterials[players.IndexOf(player)];
        }
        else
            robot = player.gameObject;

        robot.transform.position = spawnPoint.transform.position;
        robot.transform.rotation = spawnPoint.transform.rotation;
    }

	void spawnBlocks()
	{
		for(int i = 0; i < 10; i++)
		{
            spawnBlock(Random.Range(0, blockPrefabs.Length));
		}
	}
	void spawnBlock(int id)
    {
        GameObject blockObject = (GameObject)Instantiate(blockPrefabs[id], new Vector3(Random.Range(BlockSpawnMinX, BlockSpawnMaxX),
            BlockSpawnStartY, Random.Range(BlockSpawnMinZ, BlockSpawnMaxZ)), Quaternion.identity);		
		
		blockObject.AddComponent<BoxCollider>().isTrigger = true;
		blockObject.AddComponent("BoxCollision");
		blockObject.AddComponent("Light");
		
		RaycastHit hit;

		if(Physics.Raycast(blockObject.transform.position, -Vector3.up, out hit) && hit.collider.gameObject.Equals(terrain))
        {
			var distanceToGround = hit.distance;
			Vector3 dist = new Vector3(0, -distanceToGround + 1, 0);
			blockObject.transform.position += dist;
			print("Spawned a block with id: "+id+" distance to ground: "+distanceToGround);
		}
        else
        {
			Destroy(blockObject);
			print ("No block spawned, another object was in the way!");
			spawnBlock (id);
		}
	}	
	void spawnAtRandomPosition( GameObject g ){
		
	}
}
