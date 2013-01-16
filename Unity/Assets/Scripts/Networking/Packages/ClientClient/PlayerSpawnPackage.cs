using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class PlayerSpawnPackage : DataPackage
{
    public class PlayerSpawnFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 100;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            return new PlayerSpawnPackage();
        }
    }
    public static PlayerSpawnFactory factory = new PlayerSpawnFactory();

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