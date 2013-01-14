using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : PersistentMonoBehaviour
{
    public static GameManager Instance;
    public static bool IsMaster()
    {
        var v = NetworkManager.Instance.Client.GetOutgoingAddresses();
        if (v.Count == 0)
            return false;

        return v[0].Equals(Client.GetLocalIPAddress());
    }

    public int BlockSpawnStartY = 100;
    public int BlockSpawnMinX = 0, BlockSpawnMaxX = 150;
    public int BlockSpawnMinZ = 0, BlockSpawnMaxZ = 150;

	public GameObject[] blockPrefabs;
	public GameObject basePrefab;
    public Material[] baseMaterials;
    public GameObject robotPrefab;
    public Material[] robotMaterials;

    int newLevel = -1; //-1 is 'no new level'

    GameObject terrain;
    GameObject[] spawnPoints;

    List<Player> players = new List<Player>();
    List<Base> bases = new List<Base>();

    void Awake()
    {
        if (IsDuplicate())
            return;

        Instance = this;
    }

	void Start ()
    {
        OnLevelWasLoaded(Application.loadedLevel);
	}
	
	void Update ()
    {
        if (newLevel != -1)
        {
            if (newLevel == 1) //TerrainMap game world
                SetUpScene();

            newLevel = -1;
        }
	}

    void OnLevelWasLoaded(int level)
    {
        newLevel = level;
    }

    //void OnGUI()
    //{
    //    int y = 0;
    //    foreach (var v in NetworkManager.Instance.Client.GetOutgoingAddresses())
    //    {
    //        GUI.Label(new Rect(0, y, Screen.width, Screen.height), v.ToString());
    //        y += 15;
    //    }
    //}

    void SetUpScene()
    {
        terrain = GameObject.Find("Terrain");

        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        spawnBlocks();

        List<GameObject> baseSpawnPoints = GameObject.FindGameObjectsWithTag("BaseSpawnPoint").ToList();
        foreach (var v in NetworkManager.Instance.Client.GetOutgoingAddresses())
        {
            spawnRobot(v);

            GameObject bsp = baseSpawnPoints[Random.Range(0, baseSpawnPoints.Count - 1)];
            baseSpawnPoints.Remove(bsp);
            spawnBase(bsp);
        }

        while(baseSpawnPoints.Count != 0)
        {
            GameObject bsp = baseSpawnPoints[Random.Range(0, baseSpawnPoints.Count - 1)];
            baseSpawnPoints.Remove(bsp);
            spawnBase(bsp);
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

            if (!player.IsControlled)
            {
                robot.GetComponentInChildren<Camera>().enabled = false;
                foreach(var v in robot.GetComponentsInChildren<Light>().Where(x => x.name == "LaserTarget"))
                    v.enabled = false;
                foreach(var v in robot.GetComponentsInChildren<MouseLook>())
                    v.enabled = false;
            }
        }
        else
            robot = player.gameObject;

        robot.transform.position = spawnPoint.transform.position;
        robot.transform.rotation = spawnPoint.transform.rotation;
    }
    void spawnBase(GameObject baseSpawnPoint = null)
    {
        GameObject b = (GameObject)Instantiate(basePrefab, baseSpawnPoint.transform.position, baseSpawnPoint.transform.rotation);
        
        Base bs = b.GetComponent<Base>();
        bases.Add(bs);

        int matId = Random.Range(0, baseMaterials.Length / 2);
        matId *= 2;
        
        Component[] mrs = b.GetComponentsInChildren(typeof(MeshRenderer));
        foreach (MeshRenderer mr in mrs)
        {
            if (mr.material.name.Contains("Material #4"))
                mr.material = baseMaterials[matId];
            if (mr.material.name.Contains("Material #5"))
                mr.material = baseMaterials[matId + 1];
        }
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
		}
        else
        {
			Destroy(blockObject);
			spawnBlock (id);
		}
	}	
}
