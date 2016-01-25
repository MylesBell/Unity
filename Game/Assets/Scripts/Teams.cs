using UnityEngine;
using UnityEngine.Networking;

public enum TeamID {
	red, blue
}

public enum MoveDirection {
	up, down
}

public enum ProgressDirection {
	forward, backward
}

public class Teams : NetworkBehaviour, IPlayerJoin {

	public Team blueTeam, redTeam;

    public int gruntPoolSize;
    public int numberOfGruntsToSpawn;
    public int gruntSpawnInterval;
    public int heroRespawnInterval;

    private float zPositionOffset;
    public int numberOfChannels;
    public static float maxZ = 80;
    public static float minZ = 30;
    
    public static float topOffset = -5;
    public static float bottomOffset = 5;

    private bool initialised;

	void Start () {
        if (isServer) {
            initialised = false;
            zPositionOffset = ((maxZ+topOffset) - (minZ+bottomOffset)) / numberOfChannels;
            int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
            Vector3 blueBaseV = new Vector3(50, 0, 50);
            Vector3 redBaseV = new Vector3(numScreens * 100 - 50, 0, 50);
            blueTeam.Initialise(blueBaseV,zPositionOffset,numberOfChannels, numberOfGruntsToSpawn, gruntSpawnInterval, gruntPoolSize, heroRespawnInterval);
            redTeam.Initialise(redBaseV, zPositionOffset, numberOfChannels, numberOfGruntsToSpawn, gruntSpawnInterval, gruntPoolSize, heroRespawnInterval);

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
}
