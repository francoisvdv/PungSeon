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
    public GameObject flagPrefab;

    int newLevel = -1; //-1 is 'no new level'

    GameObject terrain;

    List<Player> players = new List<Player>();
    List<Base> bases = new List<Base>();
    List<Flag> flags = new List<Flag>();

    public List<Player> GetPlayers()
    {
        return players;
    }
    public Player GetPlayer(System.Net.IPAddress ip)
    {
        foreach (Player p in players)
        {
            if (p.PlayerIP.Equals(ip))
                return p;
        }
        return null;
    }
    public List<Base> GetBases()
    {
        return bases;
    }
    public Base GetPlayerBase(Player p)
    {
        foreach (Base b in bases)
        {
            if (b.Owner == p)
                return b;
        }
        return null;
    }

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

        spawnBlocks();

        List<GameObject> baseSpawnPoints = GameObject.FindGameObjectsWithTag("BaseSpawnPoint").ToList();
        int bspIndex = 0;
        foreach (var v in NetworkManager.Instance.Client.GetOutgoingAddresses())
        {
            GameObject bsp = baseSpawnPoints[bspIndex];
            baseSpawnPoints.Remove(bsp);
            spawnBase(bsp);
            bspIndex++;

            spawnRobot(v, bases[bases.Count - 1].transform.root.FindChild("RobotSpawnPoint").gameObject);
        }

        spawnFlag();
        if (NetworkManager.Instance.Client.GetOutgoingAddresses().Count >= 4)
            spawnFlag();
    }

    void spawnBlocks()
    {
        for (int i = 0; i < 10; i++)
        {
            spawnBlock(Random.Range(0, blockPrefabs.Length));
        }
    }
    void spawnBlock(int id)
    {
        GameObject blockObject = (GameObject)Instantiate(blockPrefabs[id]);
        placeRandom(blockObject, Vector3.zero);
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
                foreach (var v in robot.GetComponentsInChildren<MouseLook>())
                    v.enabled = false;
            }
        }
        else
            robot = player.gameObject;

        if (spawnPoint != null)
        {
            robot.transform.position = spawnPoint.transform.position;
            robot.transform.rotation = spawnPoint.transform.rotation;
        }
        else
            placeRandom(robot, Vector3.zero);
    }
    void spawnBase(GameObject baseSpawnPoint = null)
    {
        GameObject b = (GameObject)Instantiate(basePrefab, baseSpawnPoint.transform.position, baseSpawnPoint.transform.rotation);
        
        Base bs = b.GetComponent<Base>();
        bases.Add(bs);
    }
    void spawnFlag()
    {
        GameObject b = (GameObject)Instantiate(flagPrefab);
        placeRandom(b, new Vector3(0, 1.6f, 0));

        Flag f = b.GetComponent<Flag>();
        flags.Add(f);
    }

    void placeRandom(GameObject obj, Vector3 offset)
    {
        obj.transform.position = new Vector3(Random.Range(BlockSpawnMinX, BlockSpawnMaxX),
            BlockSpawnStartY, Random.Range(BlockSpawnMinZ, BlockSpawnMaxZ));

        RaycastHit hit;
        if (Physics.Raycast(obj.transform.position + offset, -Vector3.up, out hit) && hit.collider.gameObject.Equals(terrain))
        {
            var distanceToGround = hit.distance;
            Vector3 dist = new Vector3(0, -distanceToGround + 1, 0);
            obj.transform.position += dist;
        }
        else
            placeRandom(obj, offset);
    }
}