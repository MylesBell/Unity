using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public enum TeamID {
	red, blue
}

public class Teams : NetworkBehaviour, IPlayerJoin, IPlayerLeave {

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
    public static float maxZLeft = 270;
    public static float minZLeft = 220;
    
    public static float topOffsetRight = 3;
    public static float bottomOffsetRight = 5;
    public static float topOffsetLeft = 3;
    public static float bottomOffsetLeft = 5;

    private bool initialised;

	void Start () {
        if (isServer) {
            initialised = false;
            zPositionOffsetRight = ((maxZRight-topOffsetRight) - (minZRight+bottomOffsetRight)) / numberOfChannels;
            zPositionOffsetLeft = ((maxZLeft-topOffsetLeft) - (minZLeft+bottomOffsetLeft)) / numberOfChannels;
            int numScreensLeft = PlayerPrefs.GetInt("numberofscreens-left", 0);
            int numScreensRight = PlayerPrefs.GetInt("numberofscreens-right", 0);
            bool hasLeftLane = PlayerPrefs.GetInt("numberofscreens-left", 0) > 1;
            bool hasRightLane = PlayerPrefs.GetInt("numberofscreens-right", 0) > 1;
            int blueBaseXPosLeft = 25;
            int blueBaseXPosRight = 25;
            int redBaseXPosLeft = numScreensLeft * 100 - 25;
            int redBaseXPosRight = numScreensRight * 100 - 25;
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
            
            if (Input.GetKeyUp(KeyCode.Slash)) redTeam.CreatePlayer("id", "Test Hero");
            
            if (Input.GetKeyDown(KeyCode.I)) ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.N));
            if (Input.GetKeyDown(KeyCode.M)) ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.S));
            if (Input.GetKeyDown(KeyCode.J)) ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.W));
            if (Input.GetKeyDown(KeyCode.K)) ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.E));
            
            if (Input.GetKeyUp(KeyCode.I) || Input.GetKeyUp(KeyCode.M) || Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.K)){
                ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.NONE));
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
            int blueHeroes = blueTeam.GetNumberOfHeros();
            int redHeroes = redTeam.GetNumberOfHeros();
            if (blueHeroes < redHeroes) {
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
    
}
