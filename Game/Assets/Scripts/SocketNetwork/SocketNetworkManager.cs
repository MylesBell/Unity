using UnityEngine;
using System.Collections;
using SocketIO;

public class SocketNetworkManager : MonoBehaviour, ISocketManager  {

	public GameObject socketPrefab;

	SocketIOComponent socket;

	private string hostName = "localhost";
	private string portNumber = "1337";
	 
	// Use this for initialization
	void Start () {
		socket = createSocket (hostName, portNumber);
		socket.On("boop", TestBoopHandler);

		socket.On("close", CloseHandler);

	}
	
	public void TestBoopHandler(SocketIOEvent e){
		Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));	
	}

	public void CloseHandler(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}

	public SocketIOComponent createSocket(string host, string port){
		GameObject go = Instantiate (socketPrefab) as GameObject;
		SocketIOComponent so = go.GetComponent<SocketIOComponent> ();
		so.init (host, port);
		return so;
	}

	// Update is called once per frame
	void Update () {
		socket.Emit ("beep");
	}
}
