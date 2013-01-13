﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QQServer
{
    public class Server : INetworkListener
    {
        static Server instance;
        public static Server Instance
        {
            get
            {
                if (instance == null)
                    instance = new Server();

                return instance;
            }
        }

        public Client Client { get; private set; }

        List<Lobby> lobbies = new List<Lobby>();

        private Server()
        {
            Client = new Client();
            Client.AddListener(this);
            Client.SetMode(Client.Mode.ClientServer);
        }

        public void Start()
        {
            Client.StartConnectionListener(4551);
        }
        public void Stop()
        {
            Client.StopConnectionListener();
        }

        public void OnDataReceived(DataPackage dp)
        {
            if (dp is CreateLobbyPackage)
                OnCreateLobby((CreateLobbyPackage)dp);
            else if (dp is RequestHighscorePackage)
                OnRequestHighscore((RequestHighscorePackage)dp);
            else if (dp is RequestLobbyListPackage)
                OnRequestLobbyList((RequestLobbyListPackage)dp);
            else if (dp is SetHighscorePackage)
                OnSetHighscore((SetHighscorePackage)dp);
        }

        void OnCreateLobby(CreateLobbyPackage dp)
        {
            Lobby l = new Lobby();
            lobbies.Add(l);

            ResponsePackage rp = new ResponsePackage();
            rp.ResponseId = dp.Id;
            rp.ResponseMessage = l.LobbyId.ToString();
            Client.Write(dp.SenderTcpClient, rp);

            Console.WriteLine("Created lobby");
        }
        void OnRequestHighscore(RequestHighscorePackage dp)
        {

        }
        void OnRequestLobbyList(RequestLobbyListPackage dp)
        {
            const char lobbySeperator = '|';
            const char lobbyEntrySeperator = ';';

            string response = string.Empty;
            for(int i = 0; i < lobbies.Count; i++)
            {
                Lobby l = lobbies[i];

                string part = l.LobbyId.ToString();
                if (l.Members.Count != 0)
                    part += lobbyEntrySeperator;

                int j = 0;
                foreach (var v in l.Members)
                {
                    string address = ((IPEndPoint)v.Key.Client.RemoteEndPoint).Address.ToString();
                    part += address + lobbyEntrySeperator + v.Value.ToString();

                    if (j != l.Members.Count - 1)
                        part += lobbyEntrySeperator;

                    j++;
                }
                response += part;

                if (i != lobbies.Count - 1)
                    response += lobbySeperator;
            }

            ResponsePackage rp = new ResponsePackage();
            rp.ResponseId = dp.Id;
            rp.ResponseMessage = response;
            Client.Write(dp.SenderTcpClient, rp);

            Console.WriteLine("Lobby list sent to " + dp.SenderRemoteIPEndpoint.ToString());
        }
        void OnSetHighscore(SetHighscorePackage dp)
        {

        }
    }
}