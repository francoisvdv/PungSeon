using System;
using System.Collections;
using System.Collections.Generic;

public class TokenChangePackage : DataPackage
{
	public class TokenChangeFactory : DataPackageFactory
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
	public static TokenChangeFactory factory = new TokenChangeFactory();
	
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