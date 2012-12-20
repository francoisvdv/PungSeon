using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client
{
	private static Client instance;
	public static Client Instance
	{
		get
		{
			if(instance == null)
				instance = new Client();
			
			return instance;
		}
	}

	public Action<string> OnLog;
	
	
	TcpListener tcpListener;
	Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>(); //string contains the client's 'username'
	Queue<DataPackage> queue = new Queue<DataPackage>();
	List<NetworkListener> networkListeners = new List<NetworkListener>();
	
	string username;
	bool hasToken = false;
	TcpClient nextClient = null;
	
	byte[] readBuffer;
	StringBuilder receiveBuffer = new StringBuilder();
	Encoding encoding = Encoding.UTF8;
	
	
	
	private Client()
	{
		username = GenerateUniqueUsername();
	}
	
	string GenerateUniqueUsername()
	{
		return "John Doe " + DateTime.Now.Ticks;
	}
	
	public void AddListener(NetworkListener l)
	{
		networkListeners.Add(l);
	}
	public void RemoveListener(NetworkListener l)
	{
		if(networkListeners.Contains(l))
			networkListeners.Remove(l);
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
			clients.Add(c, GenerateUniqueUsername());
			readBuffer = new byte[c.ReceiveBufferSize];
			c.GetStream().BeginRead(readBuffer, 0, readBuffer.Length, ReceiveCallback, c);
			
			if(OnLog != null)
				OnLog("Connected: " + c.Connected + " | " + clients[c] + " | Total connections: " + clients.Count);
		}
    }
	void ReceiveCallback(IAsyncResult iar)
	{
		TcpClient c = (TcpClient)iar.AsyncState;
		
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
		receiveBuffer.Append(encoding.GetString(readBuffer, 0, read));
		for(int i = 0; i < receiveBuffer.Length; i++)
		{
			char character = receiveBuffer[i];
			if(character != Options.NewLineChar)
				continue;
			
			string data = receiveBuffer.ToString(0, i-1);
			receiveBuffer.Remove(0, i);
			i = 0;
			
			if(!string.IsNullOrEmpty(data))
				ReceiveData(data);
		}

		networkStream.BeginRead(readBuffer, 0, readBuffer.Length, ReceiveCallback, c);
	}
	void WriteCallback(IAsyncResult iar)
	{
		TcpClient client = (TcpClient)iar.AsyncState;
		
		NetworkStream networkStream = client.GetStream();
		networkStream.EndWrite(iar);
	}
	
	void Write(TcpClient client, DataPackage data)
	{
		Write(client, data.ToString());
	}
	void Write(TcpClient client, string data)
	{
		byte[] bytes = encoding.GetBytes(data);
		Write(client, bytes);
	}
	void Write(TcpClient client, byte[] data)
	{
		NetworkStream networkStream = client.GetStream();
		networkStream.BeginWrite(data, 0, data.Length, WriteCallback, client);
	}
	void WriteAll(DataPackage data)
	{	
		foreach(var kvp in clients)
		{
			Write(kvp.Key, data);
		}
	}
	void WriteAll(string data)
	{
		foreach(var kvp in clients)
		{
			Write(kvp.Key, data);
		}
	}
	void WriteAll(byte[] data)
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
		clients.Add(tcpClient, GenerateUniqueUsername());
	}
	public void SendData(DataPackage dp)
	{
		queue.Enqueue(dp);
	}
	void ReceiveData(string data)
	{
		DataPackage dp = DataPackage.FromString(data);
		foreach(NetworkListener nl in networkListeners)
		{
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
