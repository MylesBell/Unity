// SocketIO methods to handle game events;

public enum State { IDLE, PLAYING, END };

public static class SocketIOOutgoingEvents {

	public static void GameStateChange (GameState.State state)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.GameStateHandler (state);
	}

	public static void PlayerHasJoined (string playerID, TeamID teamID, GameState.State state, float playerMaxHealth, float baseMaxHealth)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.PlayerJoinHandler (playerID, teamID, state, playerMaxHealth, baseMaxHealth);
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

	public static void PlayerDied (string playerID, string respawnTimestamp)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.PlayerDied (playerID, respawnTimestamp);
	}

	public static void PlayerRespawn (string playerID)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.PlayerRespawn (playerID);
	}

	public static void BaseHealthHasChanged (string playerID, float maxHealth, float currentHealth)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.BaseHealthHasChanged (playerID, maxHealth, currentHealth);
	}

}