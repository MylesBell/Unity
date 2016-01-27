// SocketIO methods to handle game events;

public enum State { IDLE, PLAYING, END };

public static class SocketIOOutgoingEvents {

	public static void GameStateChange (GameState.State state)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.GameStateHandler (state);
	}

	public static void PlayerHasJoined (string playerID, TeamID teamID, GameState.State state)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.PlayerJoinHandler (playerID, teamID, state);
	}
    
    public static void PlayerJoinFailInvalidGameCode (string playerID)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.PlayerJoinFailInvalidGameCode (playerID);
	}
    
    public static void PlayerHasLeft (string playerID, TeamID teamID, GameState.State state)
    {
        SocketNetworkManager socketIOManager = new SocketNetworkManager ();
        socketIOManager.PlayerLeaveHandler (playerID, teamID, state);
    }
    
    public static void PlayerHealthHasChanged (string playerID, float amount) {
        SocketNetworkManager socketIOManager = new SocketNetworkManager ();
        socketIOManager.PlayerChangeHealthHandler (playerID, amount);
    }

	public static void PlayerDied (string playerID)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.PlayerDied (playerID);
	}

}