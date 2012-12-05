using System;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace netwerken
{
	public class Server
	{		
		List<List<String>> Lobby;
		
		List<List<Boolean>> Ready;	
		
		List<TcpClient> clients();
		
		public Server(){
			clients = new List<TcpClient>();
			Lobby = new List<List<String>>();
			Lobby.setCapacity(3);
			Ready = new List<List<Boolean>>();	
			Ready.setCapacity(3);
			foreach(List<String> numPlayers in Lobby){
				numPlayers = new List<String>();
				numPlayers.setCapacity(6);
			}
			foreach(List<Boolean> numReady in Ready){
				numReady = new List<String>();
				numReady.setCapacity(6);
				foreach(Boolean ready in numReady){
					ready = true;
				}
			}
		}
		
		public void Connect(){
			object state = new object();
			tcp = new TcpListener(IPAddress.Any, 4550);
			tcp.Start();
			tcp.BeginAcceptTcpClient(onAccept, tcp);
		}
		
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
				clients.Add(client);
			}
		}
		
		public void WaitRequest(){
			
		}
		
		public void HandleRequest(){
			
		}
		
		public void Start(){
			
		}
		
		public static void Main(String[] args){
				
		}
	}
}

