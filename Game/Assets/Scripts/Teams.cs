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

	Team blueTeam, redTeam;
	int numPlayers, numPlayersRed, numPlayersBlue;

    private float zPositionOffset;
    public int numberOfChannels;
    public static int maxZ = 70;
    public static int minZ = 40;
    private Vector3 blueBaseUpChannelStart, blueBaseDownChannelStart, redBaseUpChannelStart, redBaseDownChannelStart;

    Dictionary<string,GameObject> playerDict = new Dictionary<string,GameObject>();
    private LinkedList<GameObject> redGrunts = new LinkedList<GameObject>();
    private LinkedList<GameObject> blueGrunts = new LinkedList<GameObject>();
    private GameObject blueBase, redBase;
    UnitFactory unitFactory;

    private bool initialised;

	void Start () {
        if (isServer) { 
		    numPlayers = 0;
		    numPlayersBlue = 0;
		    numPlayersRed = 0;
		    unitFactory = GetComponent<UnitFactory> ();
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
            if (Input.GetKeyUp(KeyCode.P)) {
                PlayerJoin("test", "Smith");
            }
            if (Input.GetKeyUp(KeyCode.G)) {
                float zPos = getZPosition();
                TeamID teamID = TeamID.blue;
                blueGrunts.AddLast(unitFactory.CreateGrunt(teamID, GetSpawnLocation(teamID), GetZTarget(TeamID.blue, zPos), zPositionOffset));
            }
            if (Input.GetKeyUp(KeyCode.H))
            {
                float zPos = getZPosition();
                TeamID teamID = TeamID.red;
                redGrunts.AddLast(unitFactory.CreateGrunt(teamID, GetSpawnLocation(teamID), GetZTarget(TeamID.red, zPos), zPositionOffset));
            }
        }
    }

	public GameObject GetHero(string playerID) {
		GameObject hero;
		playerDict.TryGetValue(playerID, out hero);
		return hero;
	}

    private void resetGame() {
        //Destroy bases
        if (blueBase) Destroy(blueBase);
        if (redBase) Destroy(redBase);
        //Destroy Grunts
        destroyList(redGrunts);
        destroyList(blueGrunts);

        //Restart heros
        foreach (KeyValuePair<string,GameObject >  entry in playerDict) {
            if (entry.Value) {
                Hero hero = entry.Value.GetComponent<Hero>();
                TeamID team = hero.getTeamID();
                float zPos = getZPosition();
                entry.Value.transform.position = GetSpawnLocation(team);
                hero.InitialiseHero(team, hero.getHeroName(), GetZTarget(team, zPos), zPositionOffset);
            }
        }

        //Create bases

        int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
        Vector3 blueBaseV = new Vector3(50, 2, 50);
        Vector3 redBaseV = new Vector3(numScreens * 100 - 50, 2, 50);
        blueBase = unitFactory.CreateBase(TeamID.blue, blueBaseV);
        redBase = unitFactory.CreateBase(TeamID.red, redBaseV);

        initialised = true;
    }

    private Vector3 GetSpawnLocation (TeamID teamID) {
		Vector3 spawnLocation = Vector3.zero;
		if (teamID == TeamID.blue)
			spawnLocation = blueBase.transform.position + new Vector3(4,0,0);
		else
			spawnLocation = redBase.transform.position - new Vector3(4,0,0);
		return spawnLocation;
	}

	private float getZPosition(){
		int randomNumber = Random.Range(0,numberOfChannels);
		return randomNumber * zPositionOffset + minZ;
	}
	
	private Vector3 GetZTarget (TeamID teamID, float zPosition) {
		Vector3 targetLocation = Vector3.zero;
		if (teamID == TeamID.blue) {
                targetLocation = new Vector3(blueBase.transform.position.x + 4, 1, zPosition);
		} else {
            targetLocation = new Vector3(redBase.transform.position.x - 4, 1, zPosition);
        }
		return targetLocation;
	}

    private void destroyList(LinkedList<GameObject> objects) {
        while(objects.Count > 0) {
            Destroy(objects.First.Value);
            objects.RemoveFirst();
        }
    }

	#region IPlayerJoin implementation
	public void PlayerJoin (string playerID, string playerName) {
        float zPosition = getZPosition();
        TeamID teamID;
		if (numPlayersBlue < numPlayersRed) {
            teamID = TeamID.blue;
			numPlayersBlue++;
		} else {
            teamID = TeamID.red;
			numPlayersRed++;
        }
        GameObject hero = unitFactory.CreateHero(teamID, playerName, GetSpawnLocation(teamID), GetZTarget(teamID, zPosition), zPositionOffset);

        playerDict.Add (playerID, hero);
		numPlayers++;
	}
	#endregion
}
