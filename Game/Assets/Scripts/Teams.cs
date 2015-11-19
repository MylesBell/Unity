using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public enum TeamID {
	red, blue
}

public enum MoveDirection {
	up, down
}

public class Teams : NetworkBehaviour, IPlayerJoin {

	public Team blueTeam, redTeam;

    public int numberOfGruntsToSpawn;
    public int gruntSpawnInterval;

    private float zPositionOffset;
    public int numberOfChannels;
    public static int maxZ = 80;
    public static int minZ = 25;

    private bool initialised;

	void Start () {
        if (isServer) {
            initialised = false;
            zPositionOffset = (maxZ - minZ) / numberOfChannels;
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
        int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
        Vector3 blueBaseV = new Vector3(50, 2, 50);
        Vector3 redBaseV = new Vector3(numScreens * 100 - 50, 2, 50);
        blueTeam.Initialise(blueBaseV,zPositionOffset,numberOfChannels, numberOfGruntsToSpawn, gruntSpawnInterval);
        redTeam.Initialise(redBaseV, zPositionOffset, numberOfChannels, numberOfGruntsToSpawn, gruntSpawnInterval);

        initialised = true;
    }

	#region IPlayerJoin implementation
	public void PlayerJoin (string playerID, string playerName) {
		if (blueTeam.GetNumberOfHeros() < redTeam.GetNumberOfHeros()) {
            blueTeam.CreatePlayer(playerID, playerName);
		} else {
            redTeam.CreatePlayer(playerID, playerName);
        }
	}
	#endregion
}
