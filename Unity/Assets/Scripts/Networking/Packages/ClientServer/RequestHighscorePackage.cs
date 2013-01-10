using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RequestHighscorePackage : DataPackage
{
    public class RequestHighscoreFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 0;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            return new RequestHighscorePackage();
        }
    }
    public static RequestHighscoreFactory factory = new RequestHighscoreFactory();

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