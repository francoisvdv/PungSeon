using UnityEngine;
using System.Collections;
using System;

public class Flag : MonoBehaviour, INetworkListener
{
    public const float TerrainOffset = 1.8f;

    public Guid FlagId { get; set; }
    public Player Owner { get; set; }

	void Start ()
    {
        NetworkManager.Instance.Client.AddListener(this);
    }

	void Update ()
    {
        if (Owner != null)
            transform.root.position = Owner.transform.root.position + new Vector3(0, 2, 0);
	}

    public void OnDataReceived(DataPackage dp)
    {
        FlagPackage fp = dp as FlagPackage;
        if (fp == null || fp.FlagId != FlagId)
            return;

        if (fp.Event == FlagPackage.FlagEvent.PickUp)
        {
            Owner = GameManager.Instance.GetPlayer(fp.SenderRemoteIPEndpoint.Address);
        }
        else if (fp.Event == FlagPackage.FlagEvent.Drop)
        {
            Owner = null;
            transform.root.position = GameManager.Instance.getPositionOnTerrain(transform.root.position, TerrainOffset);
        }
    }
}
