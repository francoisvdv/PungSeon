using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace netwerken
{	
	
	class Client
	{
		/* The name of person using the client */
		private string username;
		
		/* The list of peer IP's*/
		private List<string> peerIP;
		
		/* The tcp connection to the server */
		private TcpClient server;
				
		/* The list of tcp connections to the server */ 
		private List<TcpClient> peers;
		
		/* The id assigned to the client by the server*/
		private int clientID; 
		
		/* Listens for peer connections*/
		private TcpListener tcp;
		
		/*
		 * Initialise the client
		 */ 
		public Client(string username) {
			this.username = username;
			peers = new List<TcpClient>();
			Console.WriteLine("start accepting");
			startlisten();
		}
		
		/*
		 * Connect to a server
		 */ 
		public void Connect(){
			server = new TcpClient();
		}
		
		/*
		 * Connect to a peer
		 */
		public void Connect(string ip, int port){
			TcpClient peer = new TcpClient();
			
			peer.BeginConnect(ip, port, onConnect, new object());
		}
		
		void onConnect(IAsyncResult iar){ /* no need to do anything here */ }
		
		/*
		 * Start listening for connections from peers 
		 */
		void startlisten(){
			object state = new object();
			tcp = new TcpListener(IPAddress.Any, 4550);
			tcp.Start();
			tcp.BeginAcceptTcpClient(onAccept, tcp);
		}
		
		/*
		 * This is executed when the connection request from another peer is accepted
		 */
		void onAccept(IAsyncResult iar){
			TcpListener l = (TcpListener) iar.AsyncState;
	        TcpClient peer;
	        try
	        {
	            peer = l.EndAcceptTcpClient(iar); //new peer
				
				Console.WriteLine("New connection! Connections: " + peers.Count);
				
	            l.BeginAcceptTcpClient(new AsyncCallback(onAccept), l);
				
	        }
	        catch (SocketException ex)
	        {
				Console.WriteLine("Error accepting TCP connection: " + ex.Message);
	            return;
	        }
	        catch (ObjectDisposedException)
	        {
	            // The listener was Stop()'d, disposing the underlying socket and
	            // triggering the completion of the callback. We're already exiting,
	            // so just return.
				Console.WriteLine("Listen canceled.");
	            return;
	        }
			
			if(peer != null && peer.Connected)
			{
				Console.WriteLine("Connected: " + peer.Connected);
				peers.Add(peer);
			}
		}
		
		/*
		 * Send queued data to server
		 */
		public void sendData(String message, String ip){
			
		}
		
		/*
		 * Label and content of the received message from server
		 */
		public void receiveData(){
						
		}
		
		/* 
		 * Returns whether the network is available 
		 */
		public Boolean networkAvailable(){
			return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
		}
		
		public static void Main()
		{
			
		}
	}
}