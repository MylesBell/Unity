using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Team : NetworkBehaviour {

    public TeamID teamID;
    private Vector3 basePosition;
    UnitFactory unitFactory;
    private bool gruntPoolInitialised;
    private GameObject teamBase;
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

    public void Initialise(Vector3 basePosition, float zPositionOffset, int numberOfChannels, int numberOfGruntsToSpawn, int spawnInterval, int gruntPoolSize, int heroRespawnInterval) {
        this.zPositionOffset = zPositionOffset;
        this.numberOfChannels = numberOfChannels;
        this.basePosition = basePosition;
        this.numberOfGruntsToSpawn = numberOfGruntsToSpawn;
        this.gruntSpawnInterval = spawnInterval;
        this.gruntPoolSize = gruntPoolSize;
        this.heroRespawnInterval = heroRespawnInterval;
        //Create base
        teamBase = unitFactory.CreateBase(BasePrefab);
        teamBase.GetComponent<Base>().InitialiseGameObject(this);

        resetTeam();
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

    private Vector3 GetSpawnLocation() {
        Vector3 spawnLocation = Vector3.zero;
        if (teamID == TeamID.blue)
            spawnLocation = teamBase.transform.position + new Vector3(4, 0, 0);
        else
            spawnLocation = teamBase.transform.position - new Vector3(4, 0, 0);
        return spawnLocation + new Vector3(0, 10, 0);
    }

    private float getZPosition() {
        int randomNumber = Random.Range(0, numberOfChannels);
        return randomNumber * zPositionOffset + Teams.minZ;
    }

    private Vector3 GetTargetPosition(float zPosition) {
        return new Vector3(teamBase.transform.position.x + (teamID == TeamID.blue ? 4 : -4) , 0, zPosition);
    }

    private void resetTeam() {
        teamBase.GetComponent<Base>().ResetGameObject(basePosition, Vector3.zero, 0.0f);

        //Restart heros
        foreach (KeyValuePair<string, GameObject> entry in playerDict) {
            if (entry.Value) {
                HeroRespawn(entry.Value);
            }
        }

        if(!gruntPoolInitialised) initialiseGruntPool();
    }

	public bool TryGetHero(string playerID, out GameObject hero) {
        return playerDict.TryGetValue(playerID, out hero);
	}

    public void CreatePlayer(string playerID, string playerName) {
        GameObject hero = unitFactory.CreateHero(HeroPrefab);
        hero.GetComponent<Hero>().InitialiseGameObject(this);
        hero.GetComponent<Hero>().setHeroName(playerName);
        hero.GetComponent<Hero>().ResetGameObject(GetSpawnLocation(), GetTargetPosition(getZPosition()), zPositionOffset);
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
        Vector3 spawnLocation =  GetSpawnLocation() + new Vector3(teamID == TeamID.blue ? i : -i, 0, 0);
        grunt.GetComponent<Grunt>().ResetGameObject(spawnLocation, GetTargetPosition(getZPosition()), zPositionOffset);
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
        Vector3 spawnLocation = GetSpawnLocation();
        hero.GetComponent<Hero>().ResetGameObject(spawnLocation, GetTargetPosition(getZPosition()), zPositionOffset);
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
