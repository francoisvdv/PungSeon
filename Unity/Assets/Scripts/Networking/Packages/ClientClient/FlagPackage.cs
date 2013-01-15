using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class FlagPackage : DataPackage
{
    public class FlagFactory : DataPackageFactory
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
            string[] split = b.Split('|');

            FlagPackage fp = new FlagPackage();
            fp.FlagId = long.Parse(split[0]);
            fp.Event = (FlagEvent)int.Parse(split[1]);

            if (fp.Event == FlagEvent.Drop || fp.Event == FlagEvent.Spawn)
                fp.Position = new Vector3(float.Parse(split[2]), float.Parse(split[3]), float.Parse(split[4]));

            return fp;
        }
    }
    public static FlagFactory factory = new FlagFactory();

    public enum FlagEvent
    {
        Spawn, PickUp, Drop, Capture
    }

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get
        {
            string r = FlagId.ToString() + "|" + ((int)Event).ToString();
            if (Event == FlagEvent.Drop || Event == FlagEvent.Spawn)
                r += "|" + Position.x.ToString() + "|" + Position.y.ToString() + "|" + Position.z.ToString();

            return r;
        }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public long FlagId { get; set; }
    public FlagEvent Event { get; set; }
    public Vector3 Position { get; set; }
}