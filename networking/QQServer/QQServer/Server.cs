using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQServer
{
    public class Server : INetworkListener
    {
        public void OnDataReceived(DataPackage dp)
        {
            if (dp is CreateLobbyPackage)
                CreateLobby((CreateLobbyPackage)dp);
            else if (dp is JoinLobbyPackage)
                JoinLobby((JoinLobbyPackage)dp);
            else if (dp is PlayerReadyPackage)
                PlayerReady((PlayerReadyPackage)dp);
            else if (dp is RequestHighscorePackage)
                RequestHighscore((RequestHighscorePackage)dp);
            else if (dp is RequestLobbyListPackage)
                RequestLobbyList((RequestLobbyListPackage)dp);
            else if (dp is SetHighscorePackage)
                SetHighscore((SetHighscorePackage)dp);
        }

        void CreateLobby(CreateLobbyPackage dp)
        {
            
        }
        void JoinLobby(JoinLobbyPackage dp)
        {

        }
        void PlayerReady(PlayerReadyPackage dp)
        {

        }
        void RequestHighscore(RequestHighscorePackage dp)
        {

        }
        void RequestLobbyList(RequestLobbyListPackage dp)
        {

        }
        void SetHighscore(SetHighscorePackage dp)
        {

        }
    }
}
