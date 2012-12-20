using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class NetworkTest : MonoBehaviour, INetworkListener
{
	void Start ()
	{
		Client.Instance.AddListener(this);
	}
	
	// Update is called once per frame
	void Update ()
	{

	}
	
	string ip = "127.0.0.1";
	string message = "Hello World!";
	void OnGUI()
	{
		ip = GUI.TextField(new Rect(0,10,200,30), ip);
		if(GUI.Button(new Rect(210, 10, 100, 100), "Connect"))
			Client.Instance.Connect(ip);
		
		message = GUI.TextField(new Rect(0, 45, 200, 30), message);
		if(GUI.Button(new Rect(315, 10, 100, 100), "Send Message"))
			Client.Instance.WriteAll(new ChatMessagePackage(message));
	}
	
	public void OnDataReceived(DataPackage dp)
	{
		ChatMessagePackage chatMessage = dp as ChatMessagePackage;
		if(chatMessage == null)
			return;
		
		print("Received chat message: " + chatMessage.Body);
	}
}
