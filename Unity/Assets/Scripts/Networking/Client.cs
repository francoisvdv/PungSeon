using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client
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
	
	
	TcpListener tcpListener;
	Dictionary<TcpClient, RemoteClient> clients = new Dictionary<TcpClient, RemoteClient>(); //string contains the client's 'username'
	Queue<DataPackage> queue = new Queue<DataPackage>();
	List<WeakReference> networkListeners = new List<WeakReference>();
	
	string username;
	bool hasToken = false;
	TcpClient nextClient = null;

	Encoding encoding = Encoding.UTF8;
	
	
	
	private Client()
	{
		username = GenerateUniqueUsername();
        SetMode(Mode.ClientServer);
	}
	
	string GenerateUniqueUsername()
	{
		return "John Doe " + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + "." + DateTime.Now.Millisecond;
	}

    public void SetMode(Mode m)
    {
        DataPackageFactory.Factories.Clear();

        if (m == Mode.ClientClient)
        {
            TokenChangePackage.RegisterFactory();
            ChatMessagePackage.RegisterFactory();
        }
        else if (m == Mode.ClientServer)
        {
            CreateLobbyPackage.RegisterFactory();
            GameStartPackage.RegisterFactory();
            JoinLobbyPackage.RegisterFactory();
            LobbyUpdatePackage.RegisterFactory();
            PlayerReadyPackage.RegisterFactory();
            RequestHighscorePackage.RegisterFactory();
            RequestLobbyListPackage.RegisterFactory();
			ResponsePackage.RegisterFactory();
            SetHighscorePackage.RegisterFactory();
        }
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
	
	public void StartTcpListener()
	{
		tcpListener = new TcpListener(IPAddress.Any, 4550);
		tcpListener.Start();
		tcpListener.BeginAcceptTcpClient(ConnectCallback, tcpListener);
		
		OnLog("Started TCP listening");
	}
	public void StopTcpListener()
	{
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
		
		if(c != null && c.Connected)
		{
			AddClient(c);
			
			if(OnLog != null)
				OnLog("Connected: " + c.Connected + " | " + clients[c] + " | Total connections: " + clients.Count);
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
		catch
		{
			//An error has occured when reading
			return;
		}
		
		//TCP is a protocol that might split messages. We store our incoming message chunks and split on newlines.
		rc.receiveBuffer.Append(encoding.GetString(rc.readBuffer, 0, read));
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
	
	public void Connect(string ip)
	{
		TcpClient tcpClient = new TcpClient();
		tcpClient.Connect(ip, 4550);
		AddClient(tcpClient);
	}
	public void SendData(DataPackage dp)
	{
		queue.Enqueue(dp);
	}
	void ReceiveData(TcpClient sender, string data)
	{
		DataPackage dp = DataPackage.FromString(data);
        dp.SenderTcpClient = sender;
        for(int i = 0; i < networkListeners.Count; i++)
		{
            INetworkListener nl = networkListeners[i].Target as INetworkListener;
            if (nl == null)
            {
                networkListeners.RemoveAt(i);
                i--;
            }
            else
			    nl.OnDataReceived(dp);
		}
	}
	
	public void Update()
	{
		if(!hasToken)
			return;
		
		while(queue.Count != 0)
		{
			DataPackage dp = queue.Dequeue();
			WriteAll(dp.ToString());
		}
		
		Write(nextClient, new TokenChangePackage());
	}
}
