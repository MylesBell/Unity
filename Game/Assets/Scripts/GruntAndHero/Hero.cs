using UnityEngine;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IDestroyableGameObject {
    private Team team;
	private string playerID;
	private TargetSelect targetSelect;
    private ComputerLane computerLane;
    [SyncVar] private bool active = false;
    
    private string playerName;
    
    public int firstUpgrade = 5;
    private int nextUpgrade = 1;
    

    public void Start() {
        gameObject.SetActive(active);
    }

	public void InitialiseGameObject(Team team) {
        if (isServer) {
            this.team = team;
            gameObject.SetActive(active);
            CmdSetActiveState(active);
        }
    }
    
    public void Update(){
        if(isServer){
            if(GameState.gameState == GameState.State.PLAYING){
                //do hero upgrade
                upgradeHero();
            }
        }
    }

	public void setplayerID(string playerIDParam){
		this.playerID = playerIDParam;
	}


	public string getplayerID(){
		return this.playerID;
	}


    public void setHeroName(string playerName) {
        this.playerName = playerName;
        CmdSetPlayerName(playerName);
        updateTextMesh(playerName);
    }

    private void updateTextMesh(string playerName) {
        transform.FindChild("HeroName").gameObject.GetComponent<TextMesh>().text = playerName;
    }
    
    private void setTextMeshDirection(ComputerLane computerLane){
        transform.FindChild("HeroName").gameObject.GetComponent<NameHero>().setTextRotation(computerLane == ComputerLane.RIGHT ? new Vector3(0,0,0) : new Vector3(0,180,0));
    }

    public void ResetGameObject(Vector3 spawnLocation, Vector3 desiredPosition, float channelOffset) {
        if (isServer) {
            active = true;
            gameObject.GetComponent<HeroMovement>().initialiseMovement(spawnLocation);
            gameObject.GetComponent<Attack>().initiliseAttack();
            
            //set Health to Max
            gameObject.GetComponent<Health>().InitialiseHealth();
            gameObject.GetComponent<Stats>().ResetKillStreak();

            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (team.GetTeamID(), desiredPosition, channelOffset);
            gameObject.SetActive(active);
            CmdSetActiveState(active);
            nextUpgrade = firstUpgrade;
        }
    }

    [Command] public void CmdSetActiveState(bool active) {
        RpcSetActive(active);
    }

    [ClientRpc]
    public void RpcSetActive(bool active) {
        gameObject.SetActive(active);
    }

    [Command] public void CmdSetPlayerName(string playerName) {
        RpcSetPlayerName(playerName);
    }

    [ClientRpc]
    public void RpcSetPlayerName(string playerName) {
        updateTextMesh(playerName);
    }
    
    [Command]
    public void CmdSetTextMeshDirection(ComputerLane computerLane) {
        RpcSetTextMeshDirection(computerLane);
    }

    [ClientRpc]
    public void RpcSetTextMeshDirection(ComputerLane computerLane) {
        setTextMeshDirection(computerLane);
    }

    public void DisableGameObject() {
        active = false;
        gameObject.SetActive(active);
        CmdSetActiveState(active);
        team.OnHeroDead(gameObject);
    }
    
    public void setComputerLane(ComputerLane computerLane){
        this.computerLane = computerLane;
        gameObject.GetComponent<HeroMovement>().setComputerLane(computerLane);
        setTextMeshDirection(computerLane);
        CmdSetTextMeshDirection(computerLane);
        setHeroName(playerName);
    }
    
    public ComputerLane getComputerLane(){
        return computerLane;
    }
    
    public void switchLane(ComputerLane newLane, Vector3 spawnLocation, Vector3 desiredPosition, float channelOffset){
        if (isServer) {
            // gameObject.GetComponent<Movement>().initialiseMovement(spawnLocation);
            gameObject.GetComponent<Attack>().initiliseAttack();
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (team.GetTeamID(), desiredPosition, channelOffset);
            setComputerLane(newLane);
        }
    }
    
    public bool hasTwoLanes(){
        return team.hasTwoLanes();
    }
    
    private void upgradeHero(){
        int killStreak = gameObject.GetComponent<Stats>().GetKillStreak();
        if(killStreak >= nextUpgrade){
            gameObject.GetComponent<Special>().UpgradeSpecials();
            nextUpgrade = nextUpgrade * 2;
        }
    }
    
    void OnBecameVisible(){
        team.OnUnitVisible(true);
    }
    void OnBecameInvisible(){
        team.OnUnitInvisible(true);
    }
}
