using System;
using System.Collections.Generic;
using System.Linq;
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
            l.Members.Add(dp.SenderTcpClient);
            lobbies.Add(l);

            ResponsePackage rp = new ResponsePackage();
            rp.ResponseId = dp.Id;
            rp.ResponseMessage = l.LobbyId.ToString();
            Client.Instance.Write(dp.SenderTcpClient, rp);
        }
        void OnRequestHighscore(RequestHighscorePackage dp)
        {

        }
        void OnRequestLobbyList(RequestLobbyListPackage dp)
        {

        }
        void OnSetHighscore(SetHighscorePackage dp)
        {

        }
    }
}