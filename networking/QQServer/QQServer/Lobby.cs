using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace QQServer
{   
    public class Lobby : INetworkListener
    {
        static int lobbyId = 0;
        public int LobbyId { get; private set; }

        public Dictionary<TcpClient, bool> Members = new Dictionary<TcpClient, bool>();
        bool freeze = false;


        public Lobby()
        {
            LobbyId = lobbyId++;

            Client.Instance.AddListener(this);
        }

        void UpdateClients(bool startGame = false)
        {
            var remove = Members.Where(x => x.Key.Connected == false);
            foreach (var v in remove)
            {
                Members.Remove(v.Key);
            }

            LobbyUpdatePackage lup = new LobbyUpdatePackage();
            lup.LobbyId = LobbyId;
            lup.Start = startGame;
            foreach (var v in Members)
            {
                string address = ((IPEndPoint)v.Key.Client.RemoteEndPoint).Address.ToString();
                if(!lup.Members.ContainsKey(address))
                    lup.Members.Add(address, v.Value);
            }

            foreach (var v in Members)
            {
                Client.Instance.Write(v.Key, lup);
                Console.WriteLine("Sent lobby update to " + ((IPEndPoint)v.Key.Client.RemoteEndPoint).Address.ToString());
            }
        }

        public void OnDataReceived(DataPackage dp)
        {
            if (dp is JoinLobbyPackage)
                OnJoinLobby((JoinLobbyPackage)dp);
            else if (dp is PlayerReadyPackage)
                OnPlayerReady((PlayerReadyPackage)dp);
        }

        void OnJoinLobby(JoinLobbyPackage dp)
        {
            if (dp.LobbyId != LobbyId)
                return;

            TcpClient clientToRemove = null;
            foreach (var v in Members)
            {
                if (dp.SenderIPEndpoint.Address.ToString() == ((IPEndPoint)v.Key.Client.RemoteEndPoint).Address.ToString())
                {
                    clientToRemove = v.Key;
                    break;
                }
            }
            if (clientToRemove != null)
            {
                Members.Remove(clientToRemove);
                Console.WriteLine("Removed " + ((IPEndPoint)clientToRemove.Client.RemoteEndPoint).ToString() + " because joining " +
                    dp.SenderIPEndpoint.ToString());
            }

            Members.Add(dp.SenderTcpClient, false);

            ResponsePackage rp = new ResponsePackage();
            rp.ResponseId = dp.Id;
            rp.ResponseMessage = (!freeze).ToString();
            Client.Instance.Write(dp.SenderTcpClient, rp);

            UpdateClients();

            Console.WriteLine(dp.SenderIPEndpoint.ToString() + " joined lobby " + LobbyId);
        }
        void OnPlayerReady(PlayerReadyPackage dp)
        {
            if (dp.LobbyId != LobbyId)
                return;

            if (!Members.ContainsKey(dp.SenderTcpClient))
                return;

            Members[dp.SenderTcpClient] = dp.Ready;

            UpdateClients();

            Console.WriteLine(dp.SenderIPEndpoint.ToString() + " ready status changed: " + dp.Ready);

            int ready = 0;
            foreach (var v in Members)
            {
                if (v.Value)
                    ready++;
            }
            if (ready != Members.Count)
                return;

            freeze = true;

            UpdateClients(true);

            Console.WriteLine("GAME START MESSAGE SENT!");
        }
    }
}
