using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : PersistentMonoBehaviour, INetworkListener
{
    public static GameManager Instance;
    public static bool IsMaster()
    {
        var v = NetworkManager.Instance.Client.GetOutgoingAddresses();
        if (v.Count == 0)
            return false;

        return v[0].Equals(Client.GetLocalIPAddress());
    }
    public static Color GetColor(int id)
    {
        switch (id)
        {
            case 0:
                return new Color(255, 0, 0);
            case 1:
                return new Color(0, 127, 14);
            case 2:
                return new Color(178, 0, 255);
            case 3:
                return new Color(182, 255, 0);
            case 4:
                return new Color(0, 255, 255);
            case 5:
                return new Color(255, 216, 0);
        }

        return Color.black;
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
    public GameObject gangnamPrefab;

    int newLevel = -1; //-1 is 'no new level'

    GameObject terrain;

    List<Player> players = new List<Player>();
    List<Base> bases = new List<Base>();
    List<Flag> flags = new List<Flag>();

    public List<Player> GetPlayers()
    {
        return players;
    }
    public List<Base> GetBases()
    {
        return bases;
    }
    public Player GetPlayer()
    {
        return GetPlayer(Client.GetLocalIPAddress());
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
    public Base GetPlayerBase(Player p)
    {
        foreach (Base b in bases)
        {
            if (b.Owner == p)
                return b;
        }
        return null;
    }
    public Flag GetFlag(System.Guid flagId)
    {
        foreach (Flag v in flags)
        {
            if (v.FlagId == flagId)
                return v;
        }
        return null;
    }
    public Flag GetFlag(Player p)
    {
        foreach (Flag v in flags)
        {
            if (v.Owner == p)
                return v;
        }
        return null;
    }
    public Color GetColor(System.Net.IPAddress ip)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].PlayerIP.Equals(ip))
                return GetColor(i);
        }
        return Color.black;
    }

    void Awake()
    {
        if (IsDuplicate())
            return;

        Instance = this;
    }

	void Start ()
    {
        NetworkManager.Instance.Client.AddListener(this);

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
        foreach (var v in NetworkManager.Instance.Client.GetOutgoingAddresses())
        {
            GameObject bsp = baseSpawnPoints[0];
            baseSpawnPoints.Remove(bsp);
            spawnBase(bsp);

            spawnRobot(v, bases[bases.Count - 1].transform.root.FindChild("RobotSpawnPoint").gameObject);
        }
        
        if (!IsMaster())
            return;

        for (int i = 0; i < 20; i++)
        {
            spawnFlag();
        }

        //spawnFlag();
        //if (NetworkManager.Instance.Client.GetOutgoingAddresses().Count >= 4)
        //    spawnFlag();
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
        blockObject.transform.position = getRandomPositionOnTerrain(0);
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
            robot.transform.position = getRandomPositionOnTerrain(0);
    }
    void spawnBase(GameObject baseSpawnPoint = null)
    {
        GameObject b = (GameObject)Instantiate(basePrefab, baseSpawnPoint.transform.position, baseSpawnPoint.transform.rotation);
        
        Base bs = b.GetComponent<Base>();
        bases.Add(bs);
    }
    void spawnFlag()
    {
        FlagPackage fp = new FlagPackage();
        fp.FlagId = System.Guid.NewGuid();
        fp.Event = FlagPackage.FlagEvent.Spawn;
        fp.Position = getRandomPositionOnTerrain(Flag.TerrainOffset);
        NetworkManager.Instance.Client.SendData(fp);
    }

    public Vector3 getRandomPositionOnTerrain(float heightOffset)
    {
        Vector3 result = new Vector3(Random.Range(BlockSpawnMinX, BlockSpawnMaxX),
            BlockSpawnStartY, Random.Range(BlockSpawnMinZ, BlockSpawnMaxZ));

        RaycastHit hit;
        if (Physics.Raycast(result + new Vector3(0, heightOffset, 0), -Vector3.up, out hit) && hit.collider.gameObject.Equals(terrain))
        {
            var distanceToGround = hit.distance;
            Vector3 dist = new Vector3(0, -distanceToGround + 1, 0);
            result += dist;
        }
        else
            return getRandomPositionOnTerrain(heightOffset);

        return result;
    }
    public Vector3 getPositionOnTerrain(Vector3 from, float heightOffset)
    {
        Vector3 result = from;

        RaycastHit hit;
        if (Physics.Raycast(result + new Vector3(0, heightOffset, 0), -Vector3.up, out hit) && hit.collider.gameObject.Equals(terrain))
        {
            var distanceToGround = hit.distance;
            Vector3 dist = new Vector3(0, -distanceToGround + 1, 0);
            result += dist;
        }

        return result;
    }

    public void OnDataReceived(DataPackage dp)
    {
        if (dp is FlagPackage)
        {
            FlagPackage fp = (FlagPackage)dp;
            if (fp.Event == FlagPackage.FlagEvent.Spawn)
            {
                GameObject b = (GameObject)Instantiate(flagPrefab);
                b.transform.position = fp.Position;

                Flag f = b.GetComponent<Flag>();
                flags.Add(f);
                f.FlagId = fp.FlagId;
            }
            else if (fp.Event == FlagPackage.FlagEvent.Capture)
            {
                Flag f = GetFlag(fp.FlagId);
                if (f != null)
                {
                    Destroy(f.transform.root.gameObject);
                    flags.Remove(f);
                }
            }
        }
    }
}