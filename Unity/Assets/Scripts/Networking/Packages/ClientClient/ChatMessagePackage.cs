using System;
using System.Collections;
using System.Collections.Generic;

public class ChatMessagePackage : DataPackage
{
    class ChatMessageFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return -1;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            return new ChatMessagePackage(b);
        }
    }
    static ChatMessageFactory factory = new ChatMessageFactory();
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
        get { return message; }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    string message;

    public ChatMessagePackage(string message)
    {
        this.message = message;
    }
}