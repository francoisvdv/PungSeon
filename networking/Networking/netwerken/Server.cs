using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace netwerken
{
	public class Server
	{	
		/* A list of lobby's and the users in them */
		private List<List<String>> Lobby;
		
		/* Users that are ready */
		private List<List<Boolean>> Ready;	
		
		/*
		 * Constructor
		 */
		public Server(){
			Clients = new List<TcpClient>();
			Lobby = new List<List<String>>();
			Ready = new List<List<Boolean>>();	
			//all empty elements by default have ready value true
			foreach(List<Boolean> clients in Ready){
				foreach(Boolean client in clients){
					client = true;
				}
			}
		}
		
		/*
		 * Listens for clients connection requests
		 */ 
		public void Connect(){
			object state = new object();
			tcp = new TcpListener(IPAddress.Any, 4550);
			tcp.Start();
			tcp.BeginAcceptTcpClient(onAccept, tcp);
		}
		
		/*
		 * When a client connection is accepted
		 */
		void onAccept(IAsyncResult iar){
			TcpListener l = (TcpListener) iar.AsyncState;
	        TcpClient client;
	        try
	        {
	            client = l.EndAcceptTcpClient(iar);
				
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
			
			if(client != null && client.Connected)
			{
				Console.WriteLine("Connected: " + client.Connected);
				Thread handleClient = new Thread(this.WaitRequest(client));
				handleClient.Start();
			}
		}
		
		/*
		 * Waits for a request from the client 
		 */
		public void WaitRequest(TcpClient client){
			StreamReader read = new StreamReader(client.GetStream());
			while(client.Connected()){
				String s;
				if(s = read.ReadLine() != null){
					HandleRequest(client, s);					
				}
			}
		}
		
		/*
		 * Processes the request of the client
		 */
		public void HandleRequest(TcpClient client, String request){
			StreamWriter writer = new StreamWriter(Client.GetStream());
			StreamWriter filewriter = new StreamWriter(new File("highscores"));
			String[] split = s.Split(',');
			switch(split[0]){
			
			case "CreateLobby": 
				//add entry to lobby
				//set all entry points to false
				break;
			case "JoinLobby": 
				//add entry to lobby
				//set ready entry to false
				break;
			case "Ready":
				//set ready true
				//broadcast to everyone in the lobby
				break;
			case "LeaveLobby": 
				//remove entry from lobby
				//set ready entry to true
				break;
			case "GetLobby":
				//send lobby list to client
				break;
			case "GetHighScores": 
				//read file get database
				break;
			case "SetHighScores":
				//write highscores to file
				break;
			}
		}
		
		/*
		 * Start a game 
		 */
		public void Start(){
			//repeatedly scan the lobby
			//if all entries in the list are ready
			//create a network by sending ip adresses
			//flush the list
		}
		
		public static void Main(String[] args){
			
		}
	}
}

