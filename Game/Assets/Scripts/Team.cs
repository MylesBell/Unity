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

    private float zPositionOffsetRight;
    private float zPositionOffsetLeft;
    private int numberOfChannels;

    public GameObject HeroPrefab;
    public GameObject BasePrefab;
    public GameObject GruntPrefab;

    private int gruntPoolSize;
    private int numberOfGruntsToSpawn;
    private int gruntSpawnInterval;
    private int heroRespawnInterval;

    private float nextGruntRespawn = 0.0f;

    private List<int> nonAttackableEnemies = new List<int>();

    void Start() {
        if (isServer) {
            unitFactory = gameObject.GetComponent<UnitFactory>();
            gruntPoolInitialised = false;
            numberOfHeros = 0;
        }
    }

    public void Initialise(bool hasLeftLane, bool hasRightLane, int positionXLeft, int positionXRight, float zPositionOffsetLeft, float zPositionOffsetRight, int numberOfChannels, int numberOfGruntsToSpawn, int spawnInterval, int gruntPoolSize, int heroRespawnInterval) {
        this.hasLeftLane = hasLeftLane;
        this.hasRightLane = hasRightLane;
        this.zPositionOffsetRight = zPositionOffsetRight;
        this.zPositionOffsetLeft = zPositionOffsetLeft;
        this.numberOfChannels = numberOfChannels;
        this.basePositionRight = new Vector3(positionXRight,0,50);
        this.basePositionLeft = new Vector3(positionXLeft,0,250);
        this.numberOfGruntsToSpawn = numberOfGruntsToSpawn;
        this.gruntSpawnInterval = spawnInterval;
        this.gruntPoolSize = gruntPoolSize;
        this.heroRespawnInterval = heroRespawnInterval;
        //Create base
        if(hasRightLane) teamBaseRight = unitFactory.CreateBase(BasePrefab);
        if(hasLeftLane) teamBaseLeft = unitFactory.CreateBase(BasePrefab);
        //Initialise GameObjects
        if(hasRightLane) teamBaseRight.GetComponent<Base>().InitialiseGameObject(this);
        if(hasLeftLane) teamBaseLeft.GetComponent<Base>().InitialiseGameObject(this);
    }

     void Update() {
        if (isServer) {
            if(GameState.gameState == GameState.State.PLAYING){
                if ((nextGruntRespawn > 0)) {
                    nextGruntRespawn -= Time.deltaTime;
                } else {
                    for (int i = 0; i < numberOfGruntsToSpawn; i++) {
                        if(hasRightLane) spawnGrunt(i, ComputerLane.RIGHT);
                        if(hasLeftLane) spawnGrunt(i, ComputerLane.LEFT);
                    }
                    nextGruntRespawn = gruntSpawnInterval;
                }
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

    private Vector3 GetSpawnLocation(float zPos, ComputerLane computerLane) {
        float xPos;
        if (teamID == TeamID.blue)
            xPos = (computerLane == ComputerLane.LEFT ? teamBaseLeft : teamBaseRight).transform.position.x + 15;
        else
            xPos = (computerLane == ComputerLane.LEFT ? teamBaseLeft : teamBaseRight).transform.position.x - 4;
        return new Vector3(xPos, 3, zPos);
    }

    private float getZPosition(ComputerLane computerLane) {
        int randomNumber = Random.Range(0, numberOfChannels);
        return computerLane == ComputerLane.LEFT ? randomNumber * zPositionOffsetLeft + Teams.minZLeft + Teams.bottomOffsetLeft : randomNumber * zPositionOffsetRight + Teams.minZRight + Teams.bottomOffsetRight;
    }

    private Vector3 GetTargetPosition(float zPosition, ComputerLane computerLane) {
        return new Vector3((computerLane == ComputerLane.LEFT ? teamBaseLeft : teamBaseRight).transform.position.x + (teamID == TeamID.blue ? 15 : -4) , 0, zPosition);
    }

    public void resetTeam() {
        if(hasRightLane) teamBaseRight.GetComponent<Base>().ResetGameObject(basePositionRight, Vector3.zero);
        if(hasLeftLane) teamBaseLeft.GetComponent<Base>().ResetGameObject(basePositionLeft, Vector3.zero);

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
        
        hero.GetComponent<Hero>().InitialiseGameObject(this);
		hero.GetComponent<Hero>().setplayerID (playerID);
        hero.GetComponent<Hero>().setHeroName(playerName);
        
        hero.GetComponent<Specials>().InitialiseSpecials();
        
        //Choose random lane
        ComputerLane computerLane = getSpawnLane();
        hero.GetComponent<Hero>().setComputerLane(computerLane);
        
        float zPos = getZPosition(computerLane);
        hero.GetComponent<Hero>().ResetGameObject(GetSpawnLocation(zPos, computerLane), GetTargetPosition(zPos, computerLane));
        playerDict.Add(playerID, hero);
        numberOfHeros++;
        
        // get chosen specials
        Specials special = hero.GetComponent<Specials>();
        int specialOneId = special.specialFiles[special.chosenNumbers[0]].identifier;
        int specialTwoId = special.specialFiles[special.chosenNumbers[1]].identifier;
        int specialThreeId = special.specialFiles[special.chosenNumbers[2]].identifier;
		SocketIOOutgoingEvents.PlayerHasJoined (playerID, GetTeamID(), GameState.gameState,
                                                hero.GetComponent<Health>().maxHealth,
                                                hasLeftLane ? teamBaseLeft.GetComponent<BaseHealth>().maxHealth :
                                                              teamBaseRight.GetComponent<BaseHealth>().maxHealth,
                                                specialOneId, specialTwoId, specialThreeId);
    }
    
    public void RemovePlayer(string playerID) {
        GameObject hero;
        TryGetHero(playerID, out hero);
        if(hero){
            playerDict.Remove(playerID);
            numberOfHeros--;
            Destroy(hero.gameObject);
            SocketIOOutgoingEvents.PlayerHasLeft (playerID, GetTeamID(), GameState.gameState);
        }
    }

    private void initialiseGruntPool() {
        for(int i = 0; i < gruntPoolSize; i++) {
            GameObject grunt = unitFactory.CreateGrunt(GruntPrefab);
            grunt.GetComponent<Grunt>().InitialiseGameObject(this);
            availableGrunts.AddLast(grunt);
        }
        gruntPoolInitialised = true;
    }

    private void spawnGrunt(int i, ComputerLane computerLane) {
        GameObject grunt = getGrunt();
        float zPos = getZPosition(computerLane);
        grunt.GetComponent<Grunt>().ResetGameObject(GetSpawnLocation(zPos, computerLane), GetTargetPosition(zPos, computerLane));
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
        ComputerLane computerLane = hero.GetComponent<Hero>().getComputerLane();
        string playerID = hero.GetComponent<Hero>().getplayerID();
        float zPos = getZPosition(computerLane);
        hero.GetComponent<Hero>().ResetGameObject(GetSpawnLocation(zPos,computerLane), GetTargetPosition(zPos,computerLane));
        SocketIOOutgoingEvents.PlayerRespawn(playerID);
    }

    public void OnHeroDead(GameObject hero) {
        lock (herosToRespawn) {
            herosToRespawn.Add(new Tuple<float, GameObject>(heroRespawnInterval, hero));
            string playerID = hero.GetComponent<Hero>().getplayerID();
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            double respawnTimeStamp = (System.DateTime.UtcNow - epochStart).TotalSeconds + heroRespawnInterval;
            print(respawnTimeStamp);
            SocketIOOutgoingEvents.PlayerDied(playerID, respawnTimeStamp.ToString("0.####"));
            hero.GetComponent<Specials>().ResetSpecials();
        }
    }

    public int GetNumberOfHeros() {
        return numberOfHeros;
    }

    public TeamID GetTeamID() {
        return teamID;
    }
    
    private ComputerLane getSpawnLane(){
        float range = Random.Range(0f, 1f);
        Debug.Log("Random number generated " + range);
        if(hasLeftLane && hasRightLane) return range >= 0.5f ? ComputerLane.LEFT : ComputerLane.RIGHT;
        if(hasLeftLane) return ComputerLane.LEFT;
        return ComputerLane.RIGHT;
    }
    
    public bool leftLaneExists(){
        return hasLeftLane;
    }
    public bool rightLaneExists(){
        return hasRightLane;
    }
    public bool hasTwoLanes(){
        return hasRightLane && hasLeftLane;
    }
    
    public void BaseHealthChange(float maxHealth, float currentHealth){
        foreach(string playerID in playerDict.Keys) {
            SocketIOOutgoingEvents.BaseHealthHasChanged(playerID, maxHealth, currentHealth);
        }
    }
    
    public void setNotAttackable(GameObject enemy){
        nonAttackableEnemies.Add(enemy.GetInstanceID());
        Debug.Log("Non attackable : " + enemy.GetInstanceID().ToString());
    }
    
    public void setAttackable(GameObject enemy){
        nonAttackableEnemies.Remove(enemy.GetInstanceID());
        Debug.Log("Attackable : " + enemy.GetInstanceID().ToString());
    }
    
    public bool isAttackable(GameObject target){
        return !(nonAttackableEnemies.Contains(target.GetInstanceID()));
    }
}