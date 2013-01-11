using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SetHighscorePackage : DataPackage
{
    public class SetHighscoreFactory : DataPackageFactory
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
    public static SetHighscoreFactory factory = new SetHighscoreFactory();

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