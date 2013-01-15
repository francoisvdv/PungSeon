using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class BaseCapturePackage : DataPackage
{
    public class BaseCaptureFactory : DataPackageFactory
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
            string[] split = b.Split('|');

            BaseCapturePackage bcp = new BaseCapturePackage();
            bcp.PlayerIP = IPAddress.Parse(split[0]);
            bcp.BaseId = int.Parse(split[1]);

            return bcp;
        }
    }
    public static BaseCaptureFactory factory = new BaseCaptureFactory();

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get { return PlayerIP.ToString() + "|" + BaseId.ToString(); }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public IPAddress PlayerIP { get; set; }
    public int BaseId { get; set; }
}