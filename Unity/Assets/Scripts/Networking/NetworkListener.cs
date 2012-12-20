using System;

public abstract class NetworkListener
{
	public abstract void OnDataReceived(DataPackage dp);
}