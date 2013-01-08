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

        Timer updateTimer = new Timer(1000);

        public Lobby()
        {
            LobbyId = lobbyId++;

            updateTimer.Elapsed += updateTimer_Elapsed;
            updateTimer.Start();

            Client.Instance.AddListener(this);
        }

        void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var remove = Members.Where(x => x.Key.Connected == false);
            foreach (var v in remove)
            {
                Members.Remove(v.Key);
            }

            LobbyUpdatePackage lup = new LobbyUpdatePackage();
            lup.LobbyId = LobbyId;
            foreach (var v in Members)
            {
                lup.Members.Add(((IPEndPoint)v.Key.Client.RemoteEndPoint).Address.ToString(), v.Value);
            }

            foreach (var v in Members)
            {
                Client.Instance.Write(v.Key, lup);
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

            Members.Add(dp.SenderTcpClient, false);

            ResponsePackage rp = new ResponsePackage();
            rp.ResponseId = dp.Id;
            Client.Instance.Write(dp.SenderTcpClient, rp);

            Console.WriteLine(dp.SenderIPEndpoint.ToString() + " joined lobby " + LobbyId);
        }
        void OnPlayerReady(PlayerReadyPackage dp)
        {
            if (dp.LobbyId != LobbyId)
                return;
        }
    }
}
