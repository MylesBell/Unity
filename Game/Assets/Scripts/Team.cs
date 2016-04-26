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
    Dictionary<int, GameObject> gruntDict = new Dictionary<int, GameObject>();
    private List<Tuple<float,GameObject>> herosToRespawn = new List<Tuple<float, GameObject>>();
    private List<Tower> capturedTowers = new List<Tower>();

    private float zPositionOffsetRight;
    private float zPositionOffsetLeft;
    private int numberOfChannels;

    public GameObject[] HeroPrefabs;
    public GameObject BasePrefab;
    public GameObject GruntPrefab;

    private int gruntPoolSize;
    private int numberOfGruntsToSpawn;
    private int gruntSpawnInterval;
    private int heroRespawnInterval;

    private float nextGruntRespawn = 0.0f;
    private int nextGruntID = 0;

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
                    foreach(Tower tower in capturedTowers){
                        TowerSpawnGrunts(tower);
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
            xPos = (computerLane == ComputerLane.LEFT ? teamBaseLeft : teamBaseRight).transform.position.x + 10;
        else
            xPos = (computerLane == ComputerLane.LEFT ? teamBaseLeft : teamBaseRight).transform.position.x - 10;
        return new Vector3(xPos, 0, zPos);
    }

    private float getZPosition(ComputerLane computerLane) {
        int randomNumber = Random.Range(0, numberOfChannels);
        return computerLane == ComputerLane.LEFT ? randomNumber * zPositionOffsetLeft + Teams.minZLeft + Teams.bottomOffsetLeft : randomNumber * zPositionOffsetRight + Teams.minZRight + Teams.bottomOffsetRight;
    }

    public void resetTeam() {
        if(hasRightLane) teamBaseRight.GetComponent<Base>().ResetGameObject(basePositionRight, ComputerLane.RIGHT);
        if(hasLeftLane) teamBaseLeft.GetComponent<Base>().ResetGameObject(basePositionLeft, ComputerLane.LEFT);
        
        // clear captured towers
        capturedTowers = new List<Tower>();
        
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
    
    public void resetBases(){
        if(hasRightLane) teamBaseRight.GetComponent<DamageText>().InitialiseDamageTextPool(ComputerLane.RIGHT);
        if(hasLeftLane) teamBaseLeft.GetComponent<DamageText>().InitialiseDamageTextPool(ComputerLane.LEFT);
    }

	public bool TryGetHero(string playerID, out GameObject hero) {
        return playerDict.TryGetValue(playerID, out hero);
	}

    public void CreatePlayer(string playerID, string playerName) {
        int index = Random.Range(0,HeroPrefabs.Length);
        GameObject hero = unitFactory.CreateHero(HeroPrefabs[index]);
        
        hero.GetComponent<Hero>().InitialiseGameObject(this);        
		hero.GetComponent<Hero>().setplayerID (playerID);
        hero.GetComponent<Hero>().setHeroName(playerName);
        
        hero.GetComponent<Specials>().InitialiseSpecials();
        hero.GetComponent<AllPlays>().InitialiseAllPlays();
        
        //Choose random lane
        ComputerLane computerLane = getSpawnLane();
        hero.GetComponent<Hero>().setComputerLane(computerLane);
        
        float zPos = getZPosition(computerLane);
        hero.GetComponent<Hero>().ResetGameObject(GetSpawnLocation(zPos, computerLane), computerLane);
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
                                                hero.GetComponent<Hero>().heroClass,
                                                specialOneId, specialTwoId, specialThreeId, computerLane);
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
            GameObject grunt = CreateGrunt();
            availableGrunts.AddLast(grunt);
        }
        gruntPoolInitialised = true;
    }

    private void spawnGrunt(int i, ComputerLane computerLane) {
        GameObject grunt = getGrunt();
        float zPos = getZPosition(computerLane);
        grunt.GetComponent<Grunt>().ResetGameObject(GetSpawnLocation(zPos, computerLane), computerLane);
    }
    
    private void TowerSpawnGrunts(Tower tower){
        GameObject grunt = getGrunt();
        float zPos = tower.transform.position.z;
        float xStartPos = tower.transform.position.x + (teamID == TeamID.blue ? 6 : -6);
        grunt.GetComponent<Grunt>().ResetGameObject(new Vector3(xStartPos,0,zPos), tower.computerLane);
    }

    private GameObject getGrunt() {
        GameObject grunt;
        lock (availableGrunts) { 
            if (availableGrunts.Count > 0) {
                grunt = availableGrunts.First.Value;
                availableGrunts.RemoveFirst();
            } else {
                grunt = CreateGrunt();
            }
        }
        return grunt;
    }
    
    private GameObject CreateGrunt(){
        GameObject grunt = unitFactory.CreateGrunt(GruntPrefab);
        grunt.GetComponent<Grunt>().InitialiseGameObject(this);
        grunt.GetComponent<Grunt>().SetID(nextGruntID);
        grunt.GetComponent<AllPlays>().InitialiseAllPlays();
        gruntDict.Add(nextGruntID, grunt);
        nextGruntID++;
        return grunt;
    }
    
    public bool TryGetGrunt(int id, out GameObject grunt) {
        return gruntDict.TryGetValue(id, out grunt);
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
        hero.GetComponent<Hero>().ResetGameObject(GetSpawnLocation(zPos,computerLane), computerLane);
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
            // resetting specials doesn't make sense with leveling up
            // hero.GetComponent<Specials>().ResetSpecials();
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
    
    public void CapturedTower(Tower tower){
        capturedTowers.Add(tower);
    }
    
    public void LostTower(Tower tower){
        capturedTowers.Remove(tower);
    }
}