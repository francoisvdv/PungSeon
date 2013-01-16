using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class FireWeaponPackage : DataPackage
{
    public class FireWeaponFactory : DataPackageFactory
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
            string[] split = b.Split('|');

            FireWeaponPackage fwp = new FireWeaponPackage();
            fwp.Enabled = bool.Parse(split[0]);
            fwp.Target = float.Parse(split[1]);
            return fwp;
        }
    }
    public static FireWeaponFactory factory = new FireWeaponFactory();

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get
        {
            return Enabled.ToString() + "|" + Target.ToString();
        }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public bool Enabled { get; set; }
    public float Target { get; set; }
}