using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RequestLobbyListPackage : DataPackage
{
    public class RequestLobbyListFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 1;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            return new RequestLobbyListPackage();
        }
    }
    public static RequestLobbyListFactory factory = new RequestLobbyListFactory();

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
