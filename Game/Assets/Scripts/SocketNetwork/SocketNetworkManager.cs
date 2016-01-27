using UnityEngine;
using SocketIO;
using UnityEngine.Networking;

public class SocketNetworkManager : NetworkBehaviour, ISocketManager  {

	public GameObject socketPrefab;

	private static SocketIOComponent socket;
	SocketIOInputEvents socketIOInputEvents;
	private string hostName = "icantmiss.com";
	private string portNumber = "1337";
	public enum State { IDLE, PLAYING, END };
	public static bool isInit = false;


	// Use this for initialization
	void Start () {
        if (isServer && !isInit) {
			socketIOInputEvents = new SocketIOInputEvents ();
			socket = createSocket (hostName, portNumber);
			socket.On ("playerJoin", PlayerJoinHandler);
            socket.On ("playerLeave", PlayerLeaveHandler);
			socket.On ("playerDirection", DirectionHandler);
			socket.On ("playerSwitchBase", PlayerSwitchBase);
			socket.On ("open", OpenHandler);
			socket.On ("close", CloseHandler);
            socket.On ("playerSpecial", PlayerSpecialHandler);
		}
	}

	public void PlayerJoinHandler(SocketIOEvent e){
        if (isServer) {
            Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));
            //check the game code
            socketIOInputEvents.PlayerJoin(e.data.GetField("uID").str, e.data.GetField("username").str, e.data.GetField("gamecode").str); // socekt io id, name
        }
	}
    
    public void PlayerLeaveHandler(SocketIOEvent e) {
        if (isServer) {
            Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));
            socketIOInputEvents.PlayerLeave(e.data.GetField("uID").str); //socket io id
        }
    }
 	
	public void DirectionHandler(SocketIOEvent e){
        if (isServer) {
		    Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));

		    // get the direction from the message
			string input = e.data.GetField("input").str;
			string playerID = e.data.GetField("uID").str;
		    if(input == "up" || input == "down"){
		    	MoveDirection dest = (input == "up" ? MoveDirection.up : MoveDirection.down);
			    socketIOInputEvents.PlayerMoveChannel(playerID, dest); // socket io id, channel direction
		    } else {
		    	ProgressDirection direction = (input == "forward" ? ProgressDirection.forward : ProgressDirection.backward);
				socketIOInputEvents.PlayerChangeProgressDirection(playerID, direction); // socket io id, progress direction
		    }
        }
	}
    
    public void PlayerSwitchBase(SocketIOEvent e) {
        if (isServer) {
            Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));
            socketIOInputEvents.PlayerSwitchBase(e.data.GetField("uID").str); //socket io id
        }
    }
	
	public void GameStateHandler(GameState.State state)
	{
		Debug.Log ("[SocketIO] State change");
		JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
		switch (state) {
		case GameState.State.IDLE:
			Debug.Log ("[SocketIO] Game is idle");
			dataJSON.AddField("state", (int)state);
			socket.Emit ("gameStateUpdate", dataJSON);
			break;
		case GameState.State.PLAYING:
			Debug.Log ("[SocketIO] Game is playing");
			dataJSON.AddField("state", (int)state);
			socket.Emit ("gameStateUpdate", dataJSON);
			break;
		case GameState.State.END:
			Debug.Log ("[SocketIO] Game is ended");
			dataJSON.AddField("state", (int)state);
			dataJSON.AddField("winner", (int)GameState.winningTeam);
			Debug.Log ("[Winner] "+(int)GameState.winningTeam);
			socket.Emit ("gameStateUpdate", dataJSON);
			break;
		}
	}

	public void PlayerJoinHandler(string playerID, TeamID teamID, GameState.State state)
	{
		Debug.Log ("[SocketIO] Player has joined");
		JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
		dataJSON.AddField("ok", 1);
		dataJSON.AddField("playerID", playerID);
		dataJSON.AddField("teamID", (int)teamID);
		dataJSON.AddField ("state", (int)state);
		socket.Emit ("gamePlayerJoined", dataJSON);
	}
    
    public void PlayerJoinFailInvalidGameCode(string playerID)
	{
		Debug.Log ("[SocketIO] Player has joined");
		JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
		dataJSON.AddField("ok", 0);
		dataJSON.AddField("playerID", playerID);
		dataJSON.AddField("msg", "Invalid game code.");
		socket.Emit ("gamePlayerJoined", dataJSON);
	}
    
	public void PlayerDied(string playerID)
	{
		Debug.Log ("[SocketIO] Player has died");
		JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
		dataJSON.AddField("playerID", playerID);
		socket.Emit ("gamePlayerDied", dataJSON);
	}
    
	public void PlayerNearBase(string playerID, bool nearBase)
	{
		Debug.Log ("[SocketIO] Player has died");
		JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
		dataJSON.AddField("playerID", playerID);
		dataJSON.AddField("nearBase", nearBase ? 1 : 0);
		socket.Emit ("playerNearBase", dataJSON);
	}

    public void PlayerLeaveHandler(string playerID, TeamID teamID, GameState.State state)
    {
        Debug.Log ("[SocketIO] Player has left");
        JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
		dataJSON.AddField("playerID", playerID);
		dataJSON.AddField("teamID", (int)teamID);
		dataJSON.AddField ("state", (int)state);
		socket.Emit ("gamePlayerLeft", dataJSON);
    }
    
    public void PlayerChangeHealthHandler(string playerID, float amount)
    {
        Debug.Log ("SocketIO] Player health changed");
        JSONObject dataJSON = new JSONObject(JSONObject.Type.OBJECT);
        dataJSON.AddField("playerID", playerID);
        dataJSON.AddField("amount", amount);
        socket.Emit ("gamePlayerChangeHealth", dataJSON);
    }
    
    public void PlayerSpecialHandler(SocketIOEvent e){
        if (isServer) {
            Debug.Log(string.Format("[name: {0}, data: {1}, decoded: {2}]", e.name, e.data, e.data.GetField("input")));
            socketIOInputEvents.PlayerUseSpecial(e.data.GetField("uID").str, SpecialType.fire);
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
