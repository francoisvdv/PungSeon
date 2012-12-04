using System;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace netwerken
{	
	
	class Client
	{
		/* The name of person using the client */
		private string username;
		
		/* The names of players that joined the network */
		private List<String> players;
		
		/* The tcp connection to the server */
		private TcpClient server = new TcpClient();
		
		/* The list of tcp connections to the server */ 
		private List<TcpClient> peers = new Array<TcpClient>();

		
		/*
		 * Initialise the client
		 */ 
		public Client(String username)
		{
			this.username = username;
			peers = new List<String>();
			players = new List<String>();
		}
				
		/******************************************************************************
		 * 
		 *   client-server methods
		 * 
		 ******************************************************************************
		
		/*
		 * Connect the client to the server
		 * 
		 * -make tcp connection
		 * -send username to be registered
		 */
		public void Connect(String ip, int port){
			server.Connect(ip, port);
		}
		
		/*
		 * Send queued data to server
		 */
		public void sendServerData(String label, String message){
				NetworkStream serverStream = server.GetStream();
				StreamWriter sw = new StreamWriter(serverStream);
		}
		
		public String[] getReceivedServerData(){
			String s = received.FindFirst().Split(',');
			lock(received){
         	  	receivedServer.RemoveFirst(System.Text.Encoding.ASCII.GetString(inStream));	
			}
			return s;
		}
		
		/*
		 * @return: label and content of the received message from server
		 */
		public void receiveServerData(){
			while(true){
				NetworkStream serverStream = server.GetStream();
				
				byte[] inStream = new byte[10025];
				
            	serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
				lock(received){
         	  		received.AddLast(System.Text.Encoding.ASCII.GetString(inStream));	
				}
			}
		}
		
		/****************************************************************************************************
		 * 
		 *    Peer 2 peer methods
		 * 
		 ****************************************************************************************************/
		
		/* 
		 * Returns whether the network is available 
		 */
		public Boolean networkAvailable(){
			return NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
		}
		
		/*
		 * Loop to continuously receive messages from peers
		 */
		public void Receive(){
			while(true){
				
			}
		}
		
		/*
		 * Loop to send queued messages
		 */
		public void Send(){
			while(true){
				if(send.Length>0){
					
					
				}
			}
		}
		
		public static void Main (string[] args)
		{
			
		}
	}
}
