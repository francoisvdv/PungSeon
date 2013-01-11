using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client : IDisposable, INetworkListener
{
	class RemoteClient
	{
		public string username;
		public byte[] readBuffer;
		public StringBuilder receiveBuffer = new StringBuilder();
	}
    public enum Mode { ClientServer, ClientClient }

	private static Client instance;
	public static Client Instance
	{
		get
		{
            if (instance == null)            
                instance = new Client();

            return instance;
		}
	}
    public static Client Create()
    {
        return new Client();
    }


	public Action<string> OnLog;

    public List<DataPackageFactory> Factories = new List<DataPackageFactory>();

	TcpListener tcpListener;
	Dictionary<TcpClient, RemoteClient> clients = new Dictionary<TcpClient, RemoteClient>(); //string contains the client's 'username'
	Queue<DataPackage> queue = new Queue<DataPackage>();
	List<WeakReference> networkListeners = new List<WeakReference>();

    List<DataPackage> receiveBuffer = new List<DataPackage>();

	string username;
	bool hasToken = false;
	TcpClient nextClient = null;

	Encoding encoding = Encoding.UTF8;
	
	
	
	private Client()
	{
		username = GenerateUniqueUsername();
        SetMode(Mode.ClientServer);
		
		AddListener(this);
	}
    public void Dispose()
    {
        StopConnectionListener();
        foreach (var v in clients)
        {
            v.Key.Close();
        }
    }

	public static string GenerateUniqueUsername()
	{
		return "John Doe " + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + "." + DateTime.Now.Millisecond;
	}
    public static string GetLocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
            }
        }
        return localIP;
    }

    public void SetMode(Mode m)
    {
        Factories.Clear();

        if (m == Mode.ClientClient)
        {
            Factories.Add(TokenChangePackage.factory);
            Factories.Add(ChatMessagePackage.factory);
            Factories.Add(PlayerMovePackage.factory);
        }
        else if (m == Mode.ClientServer)
        {
            Factories.Add(CreateLobbyPackage.factory);
            Factories.Add(JoinLobbyPackage.factory);
            Factories.Add(LobbyUpdatePackage.factory);
            Factories.Add(PlayerReadyPackage.factory);
            Factories.Add(RequestHighscorePackage.factory);
            Factories.Add(RequestLobbyListPackage.factory);
            Factories.Add(ResponsePackage.factory);
            Factories.Add(SetHighscorePackage.factory);
        }
    }
    public void SetHasToken(bool hasToken)
    {
        this.hasToken = hasToken;
    }
    public void SetNextTokenClient(TcpClient nextClient)
    {
        this.nextClient = nextClient;
    }

    public DataPackageFactory GetFactory(int id)
    {
        foreach (DataPackageFactory dpf in Factories)
        {
            if (dpf.Id == id)
                return dpf;
        }
        return null;
    }

    bool ConnectedTo(string ip, int port = 4550)
    {
        foreach (var v in clients)
        {
            if ((v.Key.GetRemoteIPEndPoint().Address.ToString() == ip && v.Key.GetRemoteIPEndPoint().Port == port) ||
                (v.Key.GetLocalIPEndPoint().Address.ToString() == ip && v.Key.GetLocalIPEndPoint().Port == port))
                return true;
        }

        return false;
    }
    bool ConnectedTo(TcpClient c)
    {
		return ConnectedTo(c.GetLocalIPEndPoint().Address.ToString(), c.GetLocalIPEndPoint().Port) ||
			ConnectedTo(c.GetRemoteIPEndPoint().Address.ToString(), c.GetRemoteIPEndPoint().Port);
    }
    TcpClient GetTcpClient(string ip, int port = 4550)
    {
        foreach (var v in clients)
        {
            if ((v.Key.GetLocalIPEndPoint().Address.ToString() == ip && v.Key.GetLocalIPEndPoint().Port == port) ||
				(v.Key.GetRemoteIPEndPoint().Address.ToString() == ip && v.Key.GetRemoteIPEndPoint().Port == port))
                return v.Key;
        }

        return null;
    }

	public void AddListener(INetworkListener l)
	{
		networkListeners.Add(new WeakReference(l));
	}
	public void RemoveListener(INetworkListener l)
	{
        for (int i = 0; i < networkListeners.Count; i++)
        {
            INetworkListener nl = networkListeners[i].Target as INetworkListener;
            if (nl != l)
                continue;

            networkListeners.RemoveAt(i);
            i--;
        }
	}
	
	public void StartConnectionListener(int port = 4550)
	{
        OnLog("Starting TCP listening...");

		tcpListener = new TcpListener(IPAddress.Any, port);
		tcpListener.Start();
		tcpListener.BeginAcceptTcpClient(ConnectCallback, tcpListener);
		
		OnLog("Started TCP listening");
	}
	public void StopConnectionListener()
	{
        if(tcpListener != null)
		    tcpListener.Stop();
		
		OnLog("Stopped TCP listening");
	}
	
	void ConnectCallback(IAsyncResult iar)
    {
        TcpListener l = (TcpListener)iar.AsyncState;
        TcpClient c;
        try
        {
            c = l.EndAcceptTcpClient(iar);
            l.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), l);
        }
        catch (SocketException ex)
        {
			if(OnLog != null)
				OnLog("Error accepting TCP connection: " + ex.Message);
			
            return;
        }
        catch (ObjectDisposedException)
        {
            // The listener was Stop()'d, disposing the underlying socket and
            // triggering the completion of the callback. We're already exiting,
            // so just return.
			if(OnLog != null)
            	OnLog("Listen canceled.");
			
            return;
        }

        if (c != null && c.Connected)
        {
            lock (clients)
            {
                if (!ConnectedTo(c))
                {
                    AddClient(c);

                    if (OnLog != null)
                    {
                        OnLog("Connected (incoming): " + c.GetRemoteIPEndPoint().ToString() +
                            " | Total connections: " + clients.Count);
                    }
                }
                else
                {
                    if (OnLog != null)
                    {
                        OnLog("Closing new (incoming) duplicate connection: " + c.GetRemoteIPEndPoint().ToString() +
                            " | Total connections: " + clients.Count);
                    }
					
                    c.Close();
                }
            }
        }
    }
	void ReceiveCallback(IAsyncResult iar)
	{
		TcpClient c = (TcpClient)iar.AsyncState;
		RemoteClient rc = clients[c];
	    
		int read;
		NetworkStream networkStream;
		try
		{
			networkStream = c.GetStream();
			read = networkStream.EndRead(iar);
		}
		catch(Exception exc)
		{
			if(OnLog != null)
				OnLog(exc.ToString());
			
			//An error has occured when reading
			return;
		}
		
		//TCP is a protocol that might split messages. We store our incoming message chunks and split on newlines.
		string chunk = encoding.GetString(rc.readBuffer, 0, read);
		rc.receiveBuffer.Append(chunk);
		for(int i = 0; i < rc.receiveBuffer.Length; i++)
		{
			char character = rc.receiveBuffer[i];
			if(character == Options.NewLineChar)
			{
				string data = rc.receiveBuffer.ToString(0, i);
				rc.receiveBuffer.Remove(0, i+1);
				i = -1;
				
				if(!string.IsNullOrEmpty(data))
					ReceiveData(c, data);
			}
		}
		
		if(rc.receiveBuffer.Length != 0)
			OnLog("ReceiveBuffer not empty");
		
		networkStream.BeginRead(rc.readBuffer, 0, rc.readBuffer.Length, ReceiveCallback, c);
	}
	void WriteCallback(IAsyncResult iar)
	{
		TcpClient client = (TcpClient)iar.AsyncState;
		
		NetworkStream networkStream = client.GetStream();
		networkStream.EndWrite(iar);
	}
	
	void AddClient(TcpClient c)
	{
		RemoteClient rc = new RemoteClient();
		rc.username = GenerateUniqueUsername();
		rc.readBuffer = new byte[c.ReceiveBufferSize];
		clients.Add(c, rc);

        c.GetStream().BeginRead(rc.readBuffer, 0, rc.readBuffer.Length, ReceiveCallback, c);
	}
    public int GetConnectionCount()
    {
        return clients.Count;
    }

	public void Write(TcpClient client, DataPackage data)
	{
		Write(client, data.ToString() + Options.NewLineChar);
	}
	public void Write(TcpClient client, string data)
	{
		byte[] bytes = encoding.GetBytes(data);
		Write(client, bytes);
	}
	public void Write(TcpClient client, byte[] data)
	{
		NetworkStream networkStream = client.GetStream();
		networkStream.BeginWrite(data, 0, data.Length, WriteCallback, client);
	}
	public void WriteAll(DataPackage data)
	{	
		foreach(var kvp in clients)
		{
			Write(kvp.Key, data);
		}
	}
	public void WriteAll(string data)
	{
		foreach(var kvp in clients)
		{
			Write(kvp.Key, data);
		}
	}
	public void WriteAll(byte[] data)
	{
		foreach(var kvp in clients)
		{
			Write(kvp.Key, data);
		}
	}
	
	public TcpClient Connect(string ip, int port = 4550)
	{
        lock (clients)
        {
            if (ConnectedTo(ip, port))
            {
                OnLog("Connected (existing connection): " + ip + " | " + port.ToString());
                return GetTcpClient(ip, port);
            }

            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);
            AddClient(tcpClient);

            if (OnLog != null)
            {
                OnLog("Connected (outgoing): " + tcpClient.GetRemoteIPEndPoint().ToString() +
                    " | Total connections: " + clients.Count);
            }

            return tcpClient;
        }
	}
	public void SendData(DataPackage dp)
	{
		queue.Enqueue(dp);
	}
	void ReceiveData(TcpClient sender, string data)
	{
		DataPackage dp = DataPackage.FromString(this, data);
        dp.SenderTcpClient = sender;
		lock (receiveBuffer)
		{
			receiveBuffer.Add(dp);
		}
	}

	public void Update()
	{
		//process received messages
		{
	        List<DataPackage> received = null;
	        lock (receiveBuffer)
	        {
	            received = new List<DataPackage>(receiveBuffer);
	            receiveBuffer.Clear();
	        }
			
	    	for (int i = 0; i < networkListeners.Count; i++)
	        {
	            INetworkListener nl = networkListeners[i].Target as INetworkListener;
		        if (nl == null)
		        {
		            networkListeners.RemoveAt(i);
		            i--;
		        }
	            else
				{
					foreach(DataPackage dp in received)
					{
						nl.OnDataReceived(dp);
					}
				}
	        }
		}
		
		foreach(var v in clients)
		{
			string s = v.Value.receiveBuffer.ToString();
		}
		
		if(!hasToken)
			return;
		
		while(queue.Count != 0)
		{
			DataPackage dp = queue.Dequeue();
			WriteAll(dp.ToString());
		}

        hasToken = false;
		Write(nextClient, new TokenChangePackage());
	}

    public void OnDataReceived(DataPackage dp)
    {
        TokenChangePackage tcp = dp as TokenChangePackage;
        if (tcp == null)
			return;
		
        hasToken = true;
		
		if(OnLog != null)
			OnLog("I (" + GetLocalIPAddress() + ") received the token from " + dp.SenderIPEndpoint.ToString());
    }
}
