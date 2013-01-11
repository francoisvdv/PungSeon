using System;
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
        List<Lobby> lobbies = new List<Lobby>();

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
            Client.Instance.Write(dp.SenderTcpClient, rp);

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

                foreach (var v in l.Members)
                {
                    string address = ((IPEndPoint)v.Key.Client.RemoteEndPoint).Address.ToString();
                    part += address + lobbyEntrySeperator + v.Value.ToString();
                }
                response += part;

                if (i != lobbies.Count - 1)
                    response += lobbySeperator;
            }

            ResponsePackage rp = new ResponsePackage();
            rp.ResponseId = dp.Id;
            rp.ResponseMessage = response;
            Client.Instance.Write(dp.SenderTcpClient, rp);

            Console.WriteLine("Lobby list sent to " + dp.SenderRemoteIPEndpoint.ToString());
        }
        void OnSetHighscore(SetHighscorePackage dp)
        {

        }
    }
}