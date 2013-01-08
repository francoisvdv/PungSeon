using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GameStartPackage : DataPackage
{
    class GameStartFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 5;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            return new GameStartPackage();
        }
    }
    static GameStartFactory factory = new GameStartFactory();
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
}