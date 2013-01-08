using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PlayerReadyPackage : DataPackage
{
    class PlayerReadyFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 4;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            PlayerReadyPackage prp = new PlayerReadyPackage();
            prp.LobbyId = int.Parse(b);
            return prp;
        }
    }
    static PlayerReadyFactory factory = new PlayerReadyFactory();
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
        get { return ""; }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public int LobbyId { get; set; }
}
