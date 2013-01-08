using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PlayerReadyPackage : DataPackage
{
    public class PlayerReadyFactory : DataPackageFactory
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
            string[] split = b.Split(Seperator);

            PlayerReadyPackage prp = new PlayerReadyPackage();
            prp.LobbyId = int.Parse(split[0]);
            prp.Ready = bool.Parse(split[1]);
            return prp;
        }
    }
    public static PlayerReadyFactory factory = new PlayerReadyFactory();
    public static void RegisterFactory()
    {
        DataPackageFactory.Factories.Add(factory);
    }

    const char Seperator = '|';

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get { return LobbyId.ToString() + Seperator + Ready.ToString(); }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public int LobbyId { get; set; }
    public bool Ready { get; set; }
}
