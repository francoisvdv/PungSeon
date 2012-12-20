using System;

public interface INetworkListener
{
	void OnDataReceived(DataPackage dp);
}