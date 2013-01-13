using UnityEngine;
using System;
using System.Collections;

public class Base : MonoBehaviour, INetworkListener
{
    void Start()
    {
        NetworkManager.Instance.Client.AddListener(this);
    }

    public void OnDataReceived(DataPackage dp)
    {
        
    }
}