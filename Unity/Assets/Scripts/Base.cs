using UnityEngine;
using System;
using System.Collections;

public class Base : MonoBehaviour, INetworkListener
{
    public int BaseId { get { return GameManager.Instance.GetBases().IndexOf(this); } }

    public Player Owner { get; set; }

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
        if (dp is BaseCapturePackage)
        {
            BaseCapturePackage bcp = (BaseCapturePackage)dp;
            
            if (bcp.BaseId == BaseId)
            {
                Owner = GameManager.Instance.GetPlayer(bcp.PlayerIP);
                int playerIndex = GameManager.Instance.GetPlayers().IndexOf(Owner);

                Component[] mrs = transform.root.gameObject.GetComponentsInChildren(typeof(MeshRenderer));
                foreach (MeshRenderer mr in mrs)
                {
                    if (mr.material.name.Contains("Material #4"))
                        mr.material = GameManager.Instance.baseMaterials[playerIndex * 2];
                    if (mr.material.name.Contains("Material #5"))
                        mr.material = GameManager.Instance.baseMaterials[playerIndex * 2 + 1];
                }
            }
        }
    }
}