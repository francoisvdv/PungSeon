using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class JoinLobbyPackage : DataPackage
{
    public class JoinLobbyFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 3;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            JoinLobbyPackage jlp = new JoinLobbyPackage();
            jlp.LobbyId = int.Parse(b);
            return jlp;
        }
    }
    public static JoinLobbyFactory factory = new JoinLobbyFactory();
    public static void RegisterFactory()
    {
        DataPackageFactory.Factories.Add(factory);
    }

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get { return LobbyId.ToString(); }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public int LobbyId { get; set; }
}