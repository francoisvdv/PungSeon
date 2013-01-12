using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class NetworkManager : MonoBehaviour
{
    static bool added;
	
	public bool ConnectToSelf = false;
	
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
        Client.Instance.SetMode(Client.Mode.ClientClient);
		Client.Instance.StartConnectionListener();
		
		if(ConnectToSelf)
		{
			TcpClient c = Client.Instance.Connect(Client.GetLocalIPAddress());
			Client.Instance.SetHasToken(true);
			Client.Instance.SetNextTokenClient(c);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		Client.Instance.Update();
	}
}