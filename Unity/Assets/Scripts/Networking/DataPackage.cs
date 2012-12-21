using System;
using System.Collections;
using System.Collections.Generic;

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