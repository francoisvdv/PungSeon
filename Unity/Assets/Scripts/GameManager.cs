using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		Client.Instance.OnLog = x => print (x);
		Client.Instance.StartTcpListener();
	}
	
	// Update is called once per frame
	void Update ()
	{
		Client.Instance.Update();
	}

}