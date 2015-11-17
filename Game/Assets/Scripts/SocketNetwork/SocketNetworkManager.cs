using UnityEngine;
using System.Collections;
using SocketIO;

public class SocketNetworkManager : MonoBehaviour, ISocketManager  {

	public GameObject socketPrefab;

	SocketIOComponent socket;
	SocketIOInputEvents socketIOInputEvents;
	private string hostName = "icantmiss.com";
	private string portNumber = "1337";
	 
	// Use this for initialization
	void Start () {
		socketIOInputEvents = new SocketIOInputEvents ();
		socket = createSocket (hostName, portNumber);
		socket.On("playerJoin", PlayerJoinHandler);
		socket.On("playerDirection", DirectionHandler);
		socket.On("boop", TestBoopHandler);
		socket.On("open", TestOpen);
		socket.On("close", CloseHandler);

	}

	public void PlayerJoinHandler(SocketIOEvent e){
		Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));	
		socketIOInputEvents.PlayerJoin("test");
	}

	public void DirectionHandler(SocketIOEvent e){
		Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));
		socketIOInputEvents.PlayerMoveChannel("test", Channel.up);
	}

	public void TestBoopHandler(SocketIOEvent e){
		Debug.Log(string.Format("[name: {0}, data: {1}]", e.name, e.data));	
	}

	public void CloseHandler(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}

	public void TestOpen(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
		JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
		dataJSON.AddField("name", "unity");
		socket.Emit ("subscribe", dataJSON);
	}


	public SocketIOComponent createSocket(string host, string port){
		GameObject go = Instantiate (socketPrefab) as GameObject;
		SocketIOComponent so = go.GetComponent<SocketIOComponent> ();
		so.init (host, port);
		return so;
	}

	// Update is called once per frame
	void Update () {


		if (Input.GetKeyUp (KeyCode.E)) {
			socket.Emit ("beep");
		}
	}
}
