using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class NetworkTest : MonoBehaviour {

	// Use this for initialization
	TcpListener tcpListener;
	List<TcpClient> clients = new List<TcpClient>();
	
	void Start ()
	{
		startListener();
		
		//System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();
		//tcpClient.Connect("127.0.0.1", 4550);

		//System.Net.Sockets.TcpClient tcpClient2 = new System.Net.Sockets.TcpClient();
		//tcpClient2.Connect("127.0.0.1", 4550);
	}

	void startListener()
	{
		object state = new object();
		
		tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 4550);
		tcpListener.Start();
		tcpListener.BeginAcceptTcpClient(OnAccept, tcpListener);
	}
	void stopListener()
	{
		tcpListener.Stop();
	}
	
	void OnAccept(IAsyncResult iar)
    {
        TcpListener l = (TcpListener) iar.AsyncState;
        TcpClient c;
        try
        {
            c = l.EndAcceptTcpClient(iar);
			
			print ("New connection! Connections: " + clients.Count);
			
            l.BeginAcceptTcpClient(new AsyncCallback(OnAccept), l);
        }
        catch (SocketException ex)
        {
            print("Error accepting TCP connection: " + ex.Message);
            return;
        }
        catch (ObjectDisposedException)
        {
            // The listener was Stop()'d, disposing the underlying socket and
            // triggering the completion of the callback. We're already exiting,
            // so just return.
            print("Listen canceled.");
            return;
        }
		
		if(c != null && c.Connected)
		{
			clients.Add (c);
			print ("Connected: " + c.Connected);
		}
    }
	
	// Update is called once per frame
	void Update () {

	}
	
	string ip = "127.0.0.1";
	string port = "4550";
	void OnGUI()
	{
		ip = GUI.TextField(new Rect(0,10,200,30), ip);
		port = GUI.TextField(new Rect(0,45,200,30), port);
		
		if(GUI.Button(new Rect(210, 10, 100, 100), "Connect"))
		{
			System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();
			
			int p;
			if(int.TryParse(port, out p))
				tcpClient.Connect(ip, p);
		}
	}
}
