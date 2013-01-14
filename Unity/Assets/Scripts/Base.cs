using UnityEngine;
using System;
using System.Collections;

public class Base : MonoBehaviour, INetworkListener
{
    void Start()
    {
        NetworkManager.Instance.Client.AddListener(this);
        
        GameObject t = GameObject.Find("Terrain");
        TerrainCollider tc = t.GetComponentInChildren<TerrainCollider>();

        Component[] colliders = GetComponentsInChildren(typeof(MeshCollider));
        foreach (Collider c in colliders)
        {
            Physics.IgnoreCollision(tc, c);
        }
    }

    public void OnDataReceived(DataPackage dp)
    {
        
    }
}