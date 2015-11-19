using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Team : NetworkBehaviour {

    public TeamID teamID;
    private Vector3 basePosition;
    UnitFactory unitFactory;
    private bool initialised;
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

    void Start() {
        unitFactory = gameObject.GetComponent<UnitFactory>();
        initialised = false;
        numberOfHeros = 0;
    } 

    public void Initialise(Vector3 basePosition, float zPositionOffset, int numberOfChannels) {
        this.zPositionOffset = zPositionOffset;
        this.numberOfChannels = numberOfChannels;
        this.basePosition = basePosition;
        resetTeam();
    }

     void Update() {
        if (isServer) {
            if (Input.GetKeyUp(KeyCode.G)) {
                GameObject grunt = getGrunt();
                grunt.transform.position = GetSpawnLocation();
                grunt.GetComponent<Grunt>().InitialiseGrunt(teamID, GetTargetPosition(getZPosition()), zPositionOffset);

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
        return spawnLocation;
    }

    private float getZPosition()
    {
        int randomNumber = Random.Range(0, numberOfChannels);
        return randomNumber * zPositionOffset + Teams.minZ;
    }

    private Vector3 GetTargetPosition(float zPosition) {
        return new Vector3(teamBase.transform.position.x + (teamID == TeamID.blue ? 4 : -4) , 1, zPosition);
    }

    private void resetTeam() {
        initialiseGruntPool();
        //Destroy bases
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

        //Create bases
        teamBase = unitFactory.CreateBase(BasePrefab, teamID, basePosition);

        initialised = true;
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
            GameObject grunt = unitFactory.CreateGrunt(GruntPrefab);
            grunt.SetActive(false);
            availableGrunts.AddLast(grunt);
        }
    }

    private GameObject getGrunt() {
        GameObject grunt;
        if (availableGrunts.Count > 0) {
            grunt = availableGrunts.First.Value;
            availableGrunts.RemoveFirst();
        } else {
            grunt = unitFactory.CreateGrunt(GruntPrefab);
        }
        grunt.SetActive(true);
        return grunt;
    }

    public int GetNumberOfHeros() {
        return numberOfHeros;
    }
}
