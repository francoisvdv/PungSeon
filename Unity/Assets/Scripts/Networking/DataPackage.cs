using System;
using System.Collections;
using System.Collections.Generic;

public abstract class DataPackageFactory
{
	public static List<DataPackageFactory> Factories = new List<DataPackageFactory>();
	public static DataPackageFactory GetFactory(int id)
	{
		foreach(DataPackageFactory dpf in Factories)
		{
			if(dpf.Id == id)
				return dpf;
		}
		return null;
	}
	
	public abstract int Id { get; }
	public abstract DataPackage CreateFromBody(string b);
}

public abstract class DataPackage
{
	public abstract int Id { get; }
	public abstract string Body { get; }
	public abstract DataPackageFactory Factory { get; }
	
	public override string ToString()
	{
		return Id + "," + Body;
	}
	
	static readonly char[] delimiter = new char[]{','};
	public static DataPackage FromString(string s)
	{
		string[] split = s.Split(delimiter, 2);
		int id;
		if(!int.TryParse(split[0], out id))
			return null;
		
		DataPackageFactory factory = DataPackageFactory.GetFactory(id);
		if(factory == null)
			return null;
		
		return factory.CreateFromBody(split[1]);
	}
}

public class TokenChangePackage : DataPackage
{
	class TokenChangeFactory : DataPackageFactory
	{
		public override int Id
		{
			get
			{
				return 0;
			}
		}
		
		public override DataPackage CreateFromBody(string b)
		{
			return new TokenChangePackage();
		}
	}
	static TokenChangeFactory factory = new TokenChangeFactory();
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

public class ChatMessagePackage : DataPackage
{
	class ChatMessageFactory : DataPackageFactory
	{
		public override int Id
		{
			get
			{
				return 1;
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