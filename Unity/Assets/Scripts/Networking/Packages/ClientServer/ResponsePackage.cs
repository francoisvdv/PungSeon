using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ResponsePackage : DataPackage
{
    public class ResponseFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 7;
            }
        }

        public static readonly char[] delimiter = new char[] { ',' };
        public override DataPackage CreateFromBody(string b)
        {
            ResponsePackage rp = new ResponsePackage();

            string[] split = b.Split(delimiter, 2);
            int rid = -1;
            int.TryParse(split[0], out rid);
            rp.ResponseId = rid;
            rp.ResponseMessage = split[1];
            return rp;
        }
    }
    public static ResponseFactory factory = new ResponseFactory();

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get { return ResponseId.ToString() + ResponseFactory.delimiter[0] + ResponseMessage; }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    //What DataPackage this is a response to
    public int ResponseId { get; set; }
    public string ResponseMessage { get; set; }
}