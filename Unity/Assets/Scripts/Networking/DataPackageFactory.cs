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