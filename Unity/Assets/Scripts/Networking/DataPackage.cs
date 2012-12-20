using System;
using System.Collections;
using System.Collections.Generic;

public abstract class DataPackageFactory
{
	public static List<DataPackageFactory> Factories;
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
	
	public string ToString()
	{
		return Id + "," + Body;
	}
	public static DataPackage FromString(string s)
	{
		string[] split = s.Split(',');
		int id;
		if(!int.TryParse(split[0], out id))
			return null;
		
		DataPackageFactory factory = DataPackageFactory.GetFactory(id);
		if(factory == null)
			return null;
		
		string body = "";
		for(int i = 1; i < split.Length; i++)
			body += split[i];
		return factory.CreateFromBody(body);
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