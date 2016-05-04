// SocketIO methods to handle game events;

public enum State { IDLE, PLAYING, END };

public static class SocketIOOutgoingEvents {
    
    static SocketNetworkManager socketIOManager = new SocketNetworkManager();
    
	public static void GameStateChange (GameState.State state)
	{
		socketIOManager.GameStateHandler (state);
	}

	public static void PlayerHasJoined (string playerID, TeamID teamID, GameState.State state, float playerMaxHealth,
        float baseMaxHealth, Hero.HeroClass heroClass, int specialOne, int specialTwo, int specialThree, ComputerLane computerLane)
	{
		socketIOManager.PlayerJoinHandler (playerID, teamID, state, playerMaxHealth, baseMaxHealth, heroClass,
            specialOne, specialTwo, specialThree, computerLane);
	}
    
    public static void PlayerJoinFailInvalidGameCode (string playerID)
	{
		socketIOManager.PlayerJoinFailInvalidGameCode (playerID);
	}
    
    public static void PlayerHasLeft (string playerID, TeamID teamID, GameState.State state)
    {
        socketIOManager.PlayerLeaveHandler (playerID, teamID, state);
    }
    
    public static void PlayerHealthHasChanged (string playerID, float amount) {
        socketIOManager.PlayerChangeHealthHandler (playerID, amount);
    }

	public static void PlayerDied (string playerID, string respawnTimestamp)
	{
		socketIOManager.PlayerDied (playerID, respawnTimestamp);
	}
    
	public static void PlayerRespawn (string playerID)
	{
		socketIOManager.PlayerRespawn (playerID);
	}

	public static void BaseHealthHasChanged (string playerID, float maxHealth, float currentHealth)
	{
		socketIOManager.BaseHealthHasChanged (playerID, maxHealth, currentHealth);
	}
    
    public static void PlayerLevelUp (string playerID, int level)
    {
        socketIOManager.PlayerLevelUpHandler(playerID, level);
    }
	
	public static void PlayerSwitchLaneHandler(string playerID, ComputerLane computerLane){
		socketIOManager.PlayerSwitchLaneHandler(playerID, computerLane);
	}
	
	public static void SendPlayerStats(Team[] teams){
		socketIOManager.SendPlayerStats(teams);
	} 
}