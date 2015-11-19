using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public enum TeamID {
	red, blue
}

public enum Channel {
	up, down
}

public class Teams : NetworkBehaviour, IPlayerJoin {

	Team blueTeam, redTeam;
	int numPlayers, numPlayersRed, numPlayersBlue;

    public float channelSeparation;
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
                Channel channel = getChannel();
                TeamID teamID = TeamID.blue;
                blueGrunts.AddLast(unitFactory.CreateGrunt(teamID, channel, GetSpawnLocation(teamID), GetChannelTarget(teamID, channel), channelSeparation));
            }
            if (Input.GetKeyUp(KeyCode.H))
            {
                Channel channel = getChannel();
                TeamID teamID = TeamID.red;
                redGrunts.AddLast(unitFactory.CreateGrunt(teamID, channel, GetSpawnLocation(teamID), GetChannelTarget(teamID, channel), channelSeparation));
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
                Channel channel = getChannel();
                entry.Value.transform.position = GetSpawnLocation(team);
                hero.InitialiseHero(team, hero.getHeroName(), channel, GetChannelTarget(team, channel), channelSeparation);
            }
        }

        //Create bases

        int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
        Vector3 blueBaseV = new Vector3(50, 2, 50);
        Vector3 redBaseV = new Vector3(numScreens * 100 - 50, 2, 50);
        blueBase = unitFactory.CreateBase(TeamID.blue, blueBaseV);
        redBase = unitFactory.CreateBase(TeamID.red, redBaseV);
        SetChannelStarts();

        initialised = true;
    }

    private void SetChannelStarts()
    {
        Vector3 blueBasePosition = blueBase.transform.position;
        Vector3 redBasePosition = redBase.transform.position;
        Vector3 baseDistance = redBasePosition - blueBasePosition;

        blueBaseUpChannelStart = blueBasePosition + new Vector3(channelSeparation, 0, channelSeparation);
        blueBaseDownChannelStart = blueBasePosition + new Vector3(channelSeparation, 0, -channelSeparation);
        redBaseUpChannelStart = redBasePosition + new Vector3(-channelSeparation, 0, channelSeparation);
        redBaseDownChannelStart = redBasePosition + new Vector3(-channelSeparation, 0, -channelSeparation);
    }

    private Vector3 GetSpawnLocation (TeamID teamID) {
		Vector3 spawnLocation = Vector3.zero;
		if (teamID == TeamID.blue)
			spawnLocation = blueBase.transform.position + new Vector3(4,0,0);
		else
			spawnLocation = redBase.transform.position - new Vector3(4,0,0);
		return spawnLocation;
	}

	private Channel getChannel(){
		int randomNumber = Random.Range(0,2);
		if (randomNumber == 0) {
			return Channel.up;
		}
		return Channel.down;
	}
	
	private Vector3 GetChannelTarget (TeamID teamID, Channel channel) {
		Vector3 targetLocation = Vector3.zero;
		if (teamID == TeamID.blue) {
			if (channel == Channel.up){
				targetLocation = blueBaseUpChannelStart;
			}else{
				targetLocation = blueBaseDownChannelStart;
			}
		} else {
			if (channel == Channel.up){
				targetLocation = redBaseUpChannelStart;
			}else{
				targetLocation = redBaseDownChannelStart;
			}
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
        Channel channel = getChannel();
        TeamID teamID;
		if (numPlayersBlue < numPlayersRed) {
            teamID = TeamID.blue;
			numPlayersBlue++;
		} else {
            teamID = TeamID.red;
			numPlayersRed++;
        }
        GameObject hero = unitFactory.CreateHero(teamID, playerName, channel, GetSpawnLocation(teamID), GetChannelTarget(teamID, channel), channelSeparation);

        playerDict.Add (playerID, hero);
		numPlayers++;
	}
	#endregion
}
