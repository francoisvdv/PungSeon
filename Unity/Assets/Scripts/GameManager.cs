using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    static bool added;

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

		Client.Instance.OnLog = x => print (x);
        Client.Instance.ConnectBack = false;
		Client.Instance.StartConnectionListener();
	}
	
	// Update is called once per frame
	void Update ()
	{
		Client.Instance.Update();
	}

}