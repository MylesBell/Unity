using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Team : NetworkBehaviour {

    public TeamID teamID;
    private Vector3 basePositionRight;
    private Vector3 basePositionLeft;
    private bool hasLeftLane;
    private bool hasRightLane;
    UnitFactory unitFactory;
    private bool gruntPoolInitialised;
    private GameObject teamBaseRight;
    private GameObject teamBaseLeft;
    private LinkedList<GameObject> availableGrunts = new LinkedList<GameObject>();

    private int numberOfHeros;

    Dictionary<string, GameObject> playerDict = new Dictionary<string, GameObject>();
    private List<Tuple<float,GameObject>> herosToRespawn = new List<Tuple<float, GameObject>>();

    private float zPositionOffset;
    private int numberOfChannels;

    public GameObject HeroPrefab;
    public GameObject BasePrefab;
    public GameObject GruntPrefab;

    private int gruntPoolSize;
    private int numberOfGruntsToSpawn;
    private int gruntSpawnInterval;
    private int heroRespawnInterval;

    private float nextGruntRespawn = 0.0f;


    void Start() {
        if (isServer) {
            unitFactory = gameObject.GetComponent<UnitFactory>();
            gruntPoolInitialised = false;
            numberOfHeros = 0;
            Random.seed = (int)Time.time;
        }
    }

    public void Initialise(bool hasLeftLane, bool hasRightLane, int positionXLeft, int positionXRight, float zPositionOffset, int numberOfChannels, int numberOfGruntsToSpawn, int spawnInterval, int gruntPoolSize, int heroRespawnInterval) {
        this.hasLeftLane = hasLeftLane;
        this.hasRightLane = hasRightLane;
        this.zPositionOffset = zPositionOffset;
        this.numberOfChannels = numberOfChannels;
        this.basePositionRight = new Vector3(positionXRight,0,50);
        this.basePositionLeft = new Vector3(positionXLeft,0,350);
        this.numberOfGruntsToSpawn = numberOfGruntsToSpawn;
        this.gruntSpawnInterval = spawnInterval;
        this.gruntPoolSize = gruntPoolSize;
        this.heroRespawnInterval = heroRespawnInterval;
        //Create base
        if(hasRightLane) {
            teamBaseRight = unitFactory.CreateBase(BasePrefab);
            teamBaseRight.GetComponent<Base>().InitialiseGameObject(this);
        }
        if(hasLeftLane) {
            teamBaseLeft = unitFactory.CreateBase(BasePrefab);
            teamBaseLeft.GetComponent<Base>().InitialiseGameObject(this);
        }
    }

     void Update() {
        if (isServer && GameState.gameState == GameState.State.PLAYING) {
            if ((nextGruntRespawn > 0)) {
                nextGruntRespawn -= Time.deltaTime;
			} else {
                for (int i = 0; i < numberOfGruntsToSpawn; i++) spawnGrunt(i);
                nextGruntRespawn = gruntSpawnInterval;
			}
            lock (herosToRespawn) {
                int itemsToRemove = 0;
                foreach (Tuple<float,GameObject> tuple in herosToRespawn) {
                    tuple.First -= Time.deltaTime;
                    if (tuple.First <= 0) {
                        HeroRespawn(tuple.Second);
                        itemsToRemove++;
                    }
                }
                if (itemsToRemove > 0) herosToRespawn.RemoveRange(0, itemsToRemove);
            }
        }
    }

    private Vector3 GetSpawnLocation(float zPos) {
        float xPos;
        if (teamID == TeamID.blue)
            xPos = teamBaseRight.transform.position.x + 2;
        else
            xPos = teamBaseRight.transform.position.x - 2;
        return new Vector3(xPos, 3, zPos);
    }

    private float getZPosition() {
        int randomNumber = Random.Range(0, numberOfChannels);
        return randomNumber * zPositionOffset + Teams.minZRight + Teams.bottomOffsetRight;
    }

    private Vector3 GetTargetPosition(float zPosition) {
        return new Vector3(teamBaseRight.transform.position.x + (teamID == TeamID.blue ? 4 : -4) , 0, zPosition);
    }

    public void resetTeam() {
        if(hasRightLane) teamBaseRight.GetComponent<Base>().ResetGameObject(basePositionRight, Vector3.zero, 0.0f);
        if(hasLeftLane) teamBaseLeft.GetComponent<Base>().ResetGameObject(basePositionLeft, Vector3.zero, 0.0f);

        //Restart heros
        foreach (KeyValuePair<string, GameObject> entry in playerDict) {
            if (entry.Value) {
                HeroRespawn(entry.Value);
            }
        }
        
        if(!gruntPoolInitialised) initialiseGruntPool();
        //make sure grunts spawn as soon as game starts
        nextGruntRespawn = -1;
    }

	public bool TryGetHero(string playerID, out GameObject hero) {
        return playerDict.TryGetValue(playerID, out hero);
	}

    public void CreatePlayer(string playerID, string playerName) {
        GameObject hero = unitFactory.CreateHero(HeroPrefab);
        float zPos = getZPosition();
        hero.GetComponent<Hero>().InitialiseGameObject(this);
        hero.GetComponent<Hero>().setHeroName(playerName);
        hero.GetComponent<Hero>().ResetGameObject(GetSpawnLocation(zPos), GetTargetPosition(zPos), zPositionOffset);
        playerDict.Add(playerID, hero);
        numberOfHeros++;
		SocketIOOutgoingEvents.PlayerHasJoined (playerID, GetTeamID(), GameState.gameState);
    }

    private void initialiseGruntPool() {
        for(int i = 0; i < gruntPoolSize; i++) {
            GameObject grunt = unitFactory.CreateGrunt(GruntPrefab);
            grunt.GetComponent<Grunt>().InitialiseGameObject(this);
            availableGrunts.AddLast(grunt);
        }
        gruntPoolInitialised = true;
    }

    private void spawnGrunt(int i) {
        GameObject grunt = getGrunt();
        float zPos = getZPosition();
        grunt.GetComponent<Grunt>().ResetGameObject(GetSpawnLocation(zPos), GetTargetPosition(zPos), zPositionOffset);
    }

    private GameObject getGrunt() {
        GameObject grunt;
        lock (availableGrunts) { 
            if (availableGrunts.Count > 0) {
                grunt = availableGrunts.First.Value;
                availableGrunts.RemoveFirst();
            } else {
                grunt = unitFactory.CreateGrunt(GruntPrefab);
                grunt.GetComponent<Grunt>().InitialiseGameObject(this);
            }
        }
        return grunt;
    }

    public void OnGruntDead(GameObject grunt) {
        lock (availableGrunts) {
            availableGrunts.AddLast(grunt);
        }
    }

    private void HeroRespawn(GameObject hero) {
        float zPos = getZPosition();
        hero.GetComponent<Hero>().ResetGameObject(GetSpawnLocation(zPos), GetTargetPosition(zPos), zPositionOffset);
    }

    public void OnHeroDead(GameObject hero) {
        lock (herosToRespawn) {
            herosToRespawn.Add(new Tuple<float, GameObject>(heroRespawnInterval, hero));
        }
    }

    public int GetNumberOfHeros() {
        return numberOfHeros;
    }

    public TeamID GetTeamID() {
        return teamID;
    }
}
