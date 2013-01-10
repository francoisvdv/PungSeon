using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LobbyUpdatePackage : DataPackage
{
    class LobbyUpdateFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 8;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            string[] split = b.Split(IdSeperator);
            int id = int.Parse(split[0]);
            bool start = bool.Parse(split[1]);

            LobbyUpdatePackage prp = new LobbyUpdatePackage();
            prp.LobbyId = id;
            prp.Start = start;
            if (split.Length > 2)
            {
                string[] entries = split[2].Split(EntrySeperator);
                for (int i = 0; i < entries.Length; i += 2)
                {
                    string ip = entries[i];
                    bool ready;
                    bool.TryParse(entries[i + 1], out ready);
                    prp.Members.Add(ip, ready);
                }
            }
            return prp;
        }
    }
    static LobbyUpdateFactory factory = new LobbyUpdateFactory();
    public static void RegisterFactory()
    {
        DataPackageFactory.Factories.Add(factory);
    }

    public const char IdSeperator = '|';
    public const char EntrySeperator = ';';

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get
        {
            string body = LobbyId.ToString() + IdSeperator + Start.ToString();
            if (Members.Count > 0)
                body += IdSeperator;

            int i = 0;
            foreach (var v in Members)
            {
                body += v.Key + EntrySeperator + v.Value.ToString();

                if (i != Members.Count - 1)
                    body += EntrySeperator;

                i++;
            }
            return body;
        }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public int LobbyId { get; set; }
    public Dictionary<string, bool> Members = new Dictionary<string, bool>();
    public bool Start { get; set; }
}