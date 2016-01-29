using UnityEngine;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement, IDestroyableGameObject {
    private Team team;
	private string playerID;
	private TargetSelect targetSelect;
    private ComputerLane computerLane;
    [SyncVar] private bool active = false;

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
            gameObject.GetComponent<Movement>().initialiseMovement(spawnLocation);
            gameObject.GetComponent<Attack>().initiliseAttack();
            //set Health to Max
            gameObject.GetComponent<Health>().InitialiseHealth();
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (team.GetTeamID(), desiredPosition, channelOffset);
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

    void onDestroy() {
        //fire event to SocketIo that hero is dead
    }

    #region IHeroMovement implementation
    public void PlayerChangeProgressDirection (ProgressDirection progressDirection)
	{
		if (isServer) {
            //the progress direction appears as right and left on the mobile app (we get forwards and backwards resp.)
            //flip it accoridngly
            if(team.GetTeamID() == TeamID.red && computerLane == ComputerLane.LEFT) progressDirection = progressDirection == ProgressDirection.backward ? ProgressDirection.forward : ProgressDirection.backward;
            if(team.GetTeamID() == TeamID.blue && computerLane == ComputerLane.RIGHT) progressDirection = progressDirection == ProgressDirection.backward ? ProgressDirection.forward : ProgressDirection.backward;
            targetSelect.SetProgressDirection(progressDirection);
        }
	}
	public void PlayerMoveChannel (MoveDirection moveDirection)
	{
        if (isServer) {
            //flip this on the left lane
            if (computerLane == ComputerLane.LEFT) moveDirection = moveDirection == MoveDirection.up ? MoveDirection.down : MoveDirection.up;
            targetSelect.MoveToZOffset(moveDirection, computerLane == ComputerLane.LEFT ? Teams.maxZLeft + Teams.topOffsetLeft : Teams.maxZRight + Teams.topOffsetRight,
                                                      computerLane == ComputerLane.LEFT ? Teams.minZLeft + Teams.bottomOffsetLeft : Teams.minZRight + Teams.bottomOffsetRight);
        }
    }
    #endregion

    public void DisableGameObject() {
        active = false;
        gameObject.SetActive(active);
        CmdSetActiveState(active);
        team.OnHeroDead(gameObject);
    }
    
    public void setComputerLane(ComputerLane computerLane){
        this.computerLane = computerLane;
        setTextMeshDirection(computerLane);
        CmdSetTextMeshDirection(computerLane);
    }
    
    public ComputerLane getComputerLane(){
        return computerLane;
    }
    
    public void switchLane(ComputerLane newLane, Vector3 spawnLocation, Vector3 desiredPosition, float channelOffset){
        if (isServer) {
            gameObject.GetComponent<Movement>().initialiseMovement(spawnLocation);
            gameObject.GetComponent<Attack>().initiliseAttack();
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (team.GetTeamID(), desiredPosition, channelOffset);
            setComputerLane(newLane);
        }
    }
}
