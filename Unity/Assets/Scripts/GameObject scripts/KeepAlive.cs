using UnityEngine;
using System.Collections;

public class KeepAlive : MonoBehaviour
{
    static bool added;

    public bool singleton = true;
    public bool Singleton
    {
        get { return singleton; }
        set { singleton = value; added = false; }
    }

	// Use this for initialization
	void Start ()
    {
        if (added)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            added = true;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
