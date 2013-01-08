using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QQServer
{   
    public class Lobby : INetworkListener
    {
        static int lobbyId = 0;
        public int LobbyId { get; private set; }

        public Dictionary<TcpClient, bool> Members = new Dictionary<TcpClient, bool>();

        public Lobby()
        {
            LobbyId = lobbyId++;

            Client.Instance.AddListener(this);
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
        }
        void OnPlayerReady(PlayerReadyPackage dp)
        {
            if (dp.LobbyId != LobbyId)
                return;
        }
    }
}
