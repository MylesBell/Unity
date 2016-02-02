using UnityEngine;
using UnityEngine.Networking;

public enum TeamID {
	red, blue
}

public class Teams : NetworkBehaviour, IPlayerJoin, IPlayerLeave, IPlayerSwitchBase {

	public Team blueTeam, redTeam;

    public int gruntPoolSize;
    public int numberOfGruntsToSpawn;
    public int gruntSpawnInterval;
    public int heroRespawnInterval;

    private float zPositionOffsetRight;
    private float zPositionOffsetLeft;
    public int numberOfChannels;
    public static float maxZRight = 80;
    public static float minZRight = 30;
    public static float maxZLeft = 370;
    public static float minZLeft = 320;
    
    public static float topOffsetRight = 3;
    public static float bottomOffsetRight = 5;
    public static float topOffsetLeft = 3;
    public static float bottomOffsetLeft = 5;

    private bool initialised;

	void Start () {
        if (isServer) {
            initialised = false;
            zPositionOffsetRight = ((maxZRight+topOffsetRight) - (minZRight+bottomOffsetRight)) / numberOfChannels;
            zPositionOffsetLeft = ((maxZLeft+topOffsetLeft) - (minZLeft+bottomOffsetLeft)) / numberOfChannels;
            int numScreensLeft = PlayerPrefs.GetInt("numberofscreens-left", 0);
            int numScreensRight = PlayerPrefs.GetInt("numberofscreens-right", 0);
            bool hasLeftLane = PlayerPrefs.GetInt("numberofscreens-left", 0) > 1;
            bool hasRightLane = PlayerPrefs.GetInt("numberofscreens-right", 0) > 1;
            int blueBaseXPosLeft = 50;
            int blueBaseXPosRight = 50;
            int redBaseXPosLeft = numScreensLeft * 100 - 50;
            int redBaseXPosRight = numScreensRight * 100 - 50;
            blueTeam.Initialise(hasLeftLane, hasRightLane, blueBaseXPosLeft, blueBaseXPosRight, zPositionOffsetLeft, zPositionOffsetRight,numberOfChannels, numberOfGruntsToSpawn, gruntSpawnInterval, gruntPoolSize, heroRespawnInterval);
            redTeam.Initialise(hasLeftLane,hasRightLane, redBaseXPosLeft, redBaseXPosRight, zPositionOffsetLeft, zPositionOffsetRight, numberOfChannels, numberOfGruntsToSpawn, gruntSpawnInterval, gruntPoolSize, heroRespawnInterval);

        }
    }

    void Update() {
        if (isServer) { 
            switch (GameState.gameState) {
                case GameState.State.IDLE:
                    if (!initialised) resetGame();
                    break;
                case GameState.State.PLAYING:
                    break;
                case GameState.State.END:
                    initialised = false;
                    break;
            }
        }
    }

	public GameObject GetHero(string playerID) {
		GameObject hero;
        if (!blueTeam.TryGetHero(playerID, out hero)) redTeam.TryGetHero(playerID, out hero);
		return hero;
	}

    private void resetGame() {
        blueTeam.resetTeam();
        redTeam.resetTeam();
        initialised = true;
    }

	#region IPlayerJoin implementation
	public void PlayerJoin (string playerID, string playerName, string gameCode) {
        
        if(PlayerPrefs.GetString("gameCode", "") == gameCode) {
            if (blueTeam.GetNumberOfHeros() < redTeam.GetNumberOfHeros()) {
                blueTeam.CreatePlayer(playerID, playerName);
            } else {
                redTeam.CreatePlayer(playerID, playerName);
            }
        } else {
	        SocketIOOutgoingEvents.PlayerJoinFailInvalidGameCode (playerID);
        }
	}
    #endregion

    #region IPlayerLeave implementation
    public void PlayerLeave(string playerID)
    {
        GameObject hero;
        if (blueTeam.TryGetHero(playerID, out hero))
            blueTeam.RemovePlayer(playerID);
        else
            redTeam.RemovePlayer(playerID);
            
    }
    #endregion

    #region IPlayerSwitchBase implementation
    public void PlayerSwitchBase(string playerID)
    {
        GameObject hero;
        if (blueTeam.TryGetHero(playerID, out hero))
            blueTeam.PlayerSwitchBase(playerID);
        else
            redTeam.PlayerSwitchBase(playerID);
    }
    #endregion
}
