using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SetHighscorePackage : DataPackage
{
    class SetHighscoreFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 6;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            return new SetHighscorePackage();
        }
    }
    static SetHighscoreFactory factory = new SetHighscoreFactory();
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