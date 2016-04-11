using UnityEngine;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IDestroyableGameObject {
    public Team team;
	private string playerID;
	private TargetSelect targetSelect;
    private ComputerLane computerLane;
    [SyncVar] private bool active = false;
    
    private string playerName;
    
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

    public void ResetGameObject(Vector3 spawnLocation, Vector3 desiredPosition) {
        if (isServer) {
            active = true;
            gameObject.GetComponent<HeroMovement>().initialiseMovement(spawnLocation);
            gameObject.GetComponent<Attack>().initiliseAttack();
            
            //set Health to Max
            gameObject.GetComponent<Health>().InitialiseHealth();
            gameObject.GetComponent<Stats>().ResetKillStreak();

            targetSelect = GetComponent<TargetSelect>();
            targetSelect.InitialiseTargetSelect (team.GetTeamID(), desiredPosition);
            gameObject.SetActive(active);
            CmdSetActiveState(active);
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
    
    public void switchLane(){
        if (isServer) {
            ComputerLane newLane = computerLane == ComputerLane.LEFT ? ComputerLane.RIGHT : ComputerLane.LEFT;
            gameObject.GetComponent<Attack>().initiliseAttack();
            targetSelect = GetComponent<TargetSelect> ();
            Vector3 desiredPosition = transform.position;
            desiredPosition.z = newLane == ComputerLane.LEFT ? 210f : 90f;
            //work out the x if the screen we're switching on is not 0 and the number of screens in each lane is not the same
            int currentScreen = (int)transform.position.x / 100;
            if(currentScreen > 0 && GraniteNetworkManager.numberOfScreens_left != GraniteNetworkManager.numberOfScreens_right) {
                desiredPosition.x = (transform.position.x % 100) + ((newLane == ComputerLane.LEFT ? GraniteNetworkManager.numberOfScreens_left : GraniteNetworkManager.numberOfScreens_right) - 1) * 100;
            }
            gameObject.GetComponent<HeroMovement>().initialiseMovement(desiredPosition);
            targetSelect.InitialiseTargetSelect (team.GetTeamID(), desiredPosition);
            setComputerLane(newLane);
        }
    }
    
    public bool hasTwoLanes(){
        return team.hasTwoLanes();
    }
}
