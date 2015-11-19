using UnityEngine;
using System.Collections;
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

    private float zPositionOffset;
    private int numberOfChannels;

    public GameObject HeroPrefab;
    public GameObject BasePrefab;
    public GameObject GruntPrefab;

    public int gruntPoolSize;

    private int numberOfGruntsToSpawn;
    private int spawnInterval;

    private float nextActionTime = 0.0f;


    void Start() {
        unitFactory = gameObject.GetComponent<UnitFactory>();
        gruntPoolInitialised = false;
        numberOfHeros = 0;
        Random.seed = (int)Time.time;
    } 

    public void Initialise(Vector3 basePosition, float zPositionOffset, int numberOfChannels, int numberOfGruntsToSpawn, int spawnInterval) {
        this.zPositionOffset = zPositionOffset;
        this.numberOfChannels = numberOfChannels;
        this.basePosition = basePosition;
        this.numberOfGruntsToSpawn = numberOfGruntsToSpawn;
        this.spawnInterval = spawnInterval;
        resetTeam();
    }

     void Update() {
        if (isServer && GameState.gameState == GameState.State.PLAYING) {
            if ((nextActionTime > 0)) {
                nextActionTime -= Time.deltaTime;
			} else {
                for (int i = 0; i < numberOfGruntsToSpawn; i++) spawnGrunt(i);
                nextActionTime = spawnInterval;
			}
        }
    }

    private Vector3 GetSpawnLocation()
    {
        Vector3 spawnLocation = Vector3.zero;
        if (teamID == TeamID.blue)
            spawnLocation = teamBase.transform.position + new Vector3(4, 2, 0);
        else
            spawnLocation = teamBase.transform.position - new Vector3(4, 2, 0);
        return spawnLocation + new Vector3(0, 10, 0);
    }

    private float getZPosition()
    {
        int randomNumber = Random.Range(0, numberOfChannels);
        return randomNumber * zPositionOffset + Teams.minZ;
    }

    private Vector3 GetTargetPosition(float zPosition) {
        return new Vector3(teamBase.transform.position.x + (teamID == TeamID.blue ? 4 : -4) , 0, zPosition);
    }

    private void resetTeam() {
        //Destroy base
        if (teamBase) Destroy(teamBase);

        //Restart heros
        foreach (KeyValuePair<string, GameObject> entry in playerDict)
        {
            if (entry.Value)
            {
                Hero hero = entry.Value.GetComponent<Hero>();
                float zPos = getZPosition();
                entry.Value.transform.position = GetSpawnLocation();
                hero.InitialiseHero(teamID, hero.getHeroName(), GetTargetPosition(zPos), zPositionOffset);
            }
        }

        //Create base
        teamBase = unitFactory.CreateBase(BasePrefab, teamID, basePosition);

        if(!gruntPoolInitialised) initialiseGruntPool();
    }

	public bool TryGetHero(string playerID, out GameObject hero) {
        return playerDict.TryGetValue(playerID, out hero);
	}

    public void CreatePlayer(string playerID, string playerName) {
        GameObject hero = unitFactory.CreateHero(HeroPrefab, teamID, playerName, GetSpawnLocation(), GetTargetPosition(getZPosition()), zPositionOffset);
        playerDict.Add(playerID, hero);
        numberOfHeros++;
    }

    private void initialiseGruntPool() {
        for(int i = 0; i < gruntPoolSize; i++) {
            GameObject grunt = unitFactory.CreateGrunt(GruntPrefab, GetSpawnLocation());
            availableGrunts.AddLast(grunt);
        }
        gruntPoolInitialised = true;
    }

    private void spawnGrunt(int i) {
        GameObject grunt = getGrunt();
        grunt.transform.position = GetSpawnLocation() + new Vector3(teamID == TeamID.blue ? i : -i, 0, 0);
        grunt.GetComponent<Grunt>().InitialiseGrunt(teamID, GetTargetPosition(getZPosition()), zPositionOffset);
    }

    private GameObject getGrunt() {
        GameObject grunt;
        if (availableGrunts.Count > 0) {
            grunt = availableGrunts.First.Value;
            availableGrunts.RemoveFirst();
        } else {
            grunt = unitFactory.CreateGrunt(GruntPrefab, GetSpawnLocation());
        }
        return grunt;
    }

    public int GetNumberOfHeros() {
        return numberOfHeros;
    }
}
