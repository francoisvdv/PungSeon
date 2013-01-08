using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CreateLobbyPackage : DataPackage
{
    class CreateLobbyFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 2;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            return new CreateLobbyPackage();
        }
    }
    static CreateLobbyFactory factory = new CreateLobbyFactory();
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