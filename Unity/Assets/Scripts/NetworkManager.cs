using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class NetworkManager : MonoBehaviour
{
	public bool ConnectToSelf = false;
	
	void Start()
	{
		Client.Instance.OnLog = x => print (x);
        Client.Instance.SetMode(Client.Mode.ClientClient);
		Client.Instance.StartConnectionListener();
		
		if(ConnectToSelf)
		{
			TcpClient c = Client.Instance.Connect(Client.GetLocalIPAddress().ToString());
			Client.Instance.SetHasToken(true);
			Client.Instance.SetNextTokenClient(c);
		}
	}
	
	void FixedUpdate()
	{
		Client.Instance.Update();
	}
}