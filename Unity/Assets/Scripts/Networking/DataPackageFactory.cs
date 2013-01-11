using System;
using System.Collections;
using System.Collections.Generic;

public abstract class DataPackageFactory
{	
	public abstract int Id { get; }
	public abstract DataPackage CreateFromBody(string b);
}