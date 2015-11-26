using UnityEngine;
using SocketIO;
using UnityEngine.Networking;

public class SocketNetworkManager : NetworkBehaviour, ISocketManager  {

	public GameObject socketPrefab;

	SocketIOComponent socket;
	SocketIOInputEvents socketIOInputEvents;
	private string hostName = "icantmiss.com";
	private string portNumber = "1337";
	 
	// Use this for initialization
	void Start () {
        if (isServer) {
            socketIOInputEvents = new SocketIOInputEvents();
            socket = createSocket(hostName, portNumber);
            socket.On("playerJoin", PlayerJoinHandler);
            socket.On("playerDirection", DirectionHandler);
            socket.On("boop", TestBoopHandler);
            socket.On("open", TestOpen);
            socket.On("close", CloseHandler);
        }

	}

	public void PlayerJoinHandler(SocketIOEvent e){
        if (isServer) {
            Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));
            socketIOInputEvents.PlayerJoin(e.data.GetField("uID").str, e.data.GetField("username").str); // socekt io id, name
        }
	}

	public void DirectionHandler(SocketIOEvent e){
        if (isServer) {
		    Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));

		    // get the direction from the message
		    MoveDirection dest;
		    if(e.data.GetField("input").str == "left"){
		    	dest = MoveDirection.up;
		    } else {
		    	dest = MoveDirection.down;
		    }
            
		    socketIOInputEvents.PlayerMoveChannel(e.data.GetField("uID").str, dest); // socket io id, channel direction
        }
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
