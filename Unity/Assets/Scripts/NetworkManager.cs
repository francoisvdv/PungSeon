using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class NetworkManager : PersistentMonoBehaviour
{
    public static NetworkManager Instance;

    public Client Client { get; private set; }

	public bool ConnectToSelf = false;

    void Awake()
    {
        if (IsDuplicate())
            return;

        Instance = this;

        Client = new Client();
        //Client.OnLog = x => print(x);
        Client.SetMode(Client.Mode.ClientClient);
        Client.StartConnectionListener();
        
        if (ConnectToSelf)
        {
            TcpClient c = Client.Connect(Client.GetLocalIPAddress());
            Client.SetHasToken(true);
            Client.SetNextTokenClient(c);
        }
    }

	void FixedUpdate()
	{
		Client.Update();
	}
}