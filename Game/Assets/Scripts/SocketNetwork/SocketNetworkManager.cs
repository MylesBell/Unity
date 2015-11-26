using UnityEngine;
using System.Collections;
using SocketIO;
using UnityEngine.Networking;

public class SocketNetworkManager : NetworkBehaviour, ISocketManager  {

	public GameObject socketPrefab;

	private static SocketIOComponent socket;
	SocketIOInputEvents socketIOInputEvents;
	private string hostName = "127.0.0.1";
	private string portNumber = "1337";
	public enum State { IDLE, PLAYING, END };
	public static bool isInit = false;


	// Use this for initialization
	void Start () {
        if (isServer && !isInit) {
			socketIOInputEvents = new SocketIOInputEvents ();
			socket = createSocket (hostName, portNumber);
			socket.On ("playerJoin", PlayerJoinHandler);
			socket.On ("playerDirection", DirectionHandler);
			socket.On ("open", OpenHandler);
			socket.On ("close", CloseHandler);
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
	
	public void GameStateHandler(GameState.State state)
	{
		Debug.Log ("[SocketIO] State change");
		JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
		switch (state) {
			case GameState.State.IDLE:
				Debug.Log ("[SocketIO] Game is idle");
				dataJSON.AddField("state", "idle");
				socket.Emit ("gameStateUpdate", dataJSON);
				break;
			case GameState.State.PLAYING:
				Debug.Log ("[SocketIO] Game is playing");
				dataJSON.AddField("state", "playing");
				socket.Emit ("gameStateUpdate", dataJSON);
				break;
			case GameState.State.END:
				Debug.Log ("[SocketIO] Game is ended");
				dataJSON.AddField("state", "end");
				socket.Emit ("gameStateUpdate", dataJSON);
				break;
		}
	}

	public void CloseHandler(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}

	public void OpenHandler(SocketIOEvent e)
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
	
}
