using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class PlayerHealthPackage : DataPackage
{
    public class PlayerHealthFactory : DataPackageFactory
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
            string[] split = b.Split('|');

            PlayerHealthPackage fp = new PlayerHealthPackage();
            fp.PlayerIP = IPAddress.Parse(split[0]);
            fp.Value = int.Parse(split[1]);
            fp.Hit = bool.Parse(split[2]);
            return fp;
        }
    }
    public static PlayerHealthFactory factory = new PlayerHealthFactory();

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get
        {
            string r = PlayerIP.ToString() + "|" + Value.ToString() + "|" + Hit.ToString();
            return r;
        }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public IPAddress PlayerIP { get; set; }
    public int Value { get; set; }
    public bool Hit { get; set; }
}