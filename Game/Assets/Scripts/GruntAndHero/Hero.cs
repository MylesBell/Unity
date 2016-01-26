using UnityEngine;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement, IDestroyableGameObject {
    private Team team;
	private TargetSelect targetSelect;
    private CreateTerrain.ComputerLane computerLane;
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

    public void setHeroName(string playerName) {
        CmdSetPlayerName(playerName);
        updateTextMesh(playerName);
    }

    private void updateTextMesh(string playerName) {
        transform.FindChild("HeroName").gameObject.GetComponent<TextMesh>().text = playerName;
    }

    public void ResetGameObject(Vector3 spawnLocation, Vector3 desiredPosition, float channelOffset) {
        if (isServer) {
            active = true;
            gameObject.GetComponent<Movement>().initialiseMovement(spawnLocation);
            gameObject.GetComponent<Attack>().initiliseAttack();
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
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

    void onDestroy() {
        //fire event to SocketIo that hero is dead
    }

    #region IHeroMovement implementation
    public void PlayerChangeProgressDirection (ProgressDirection progressDirection)
	{
		if (isServer) {
            targetSelect.SetProgressDirection(progressDirection);
        }
	}
	public void PlayerMoveChannel (MoveDirection moveDirection)
	{
        if (isServer) {
            targetSelect.MoveToZOffset(moveDirection, computerLane == CreateTerrain.ComputerLane.LEFT ? Teams.maxZLeft + Teams.topOffsetLeft : Teams.maxZRight + Teams.topOffsetRight,
                                                      computerLane == CreateTerrain.ComputerLane.LEFT ? Teams.minZLeft + Teams.bottomOffsetLeft : Teams.minZRight + Teams.bottomOffsetRight);
        }
    }
    #endregion

    public void DisableGameObject() {
        active = false;
        gameObject.SetActive(active);
        CmdSetActiveState(active);
        team.OnHeroDead(gameObject);
    }
    
    public void setComputerLane(CreateTerrain.ComputerLane computerLane){
        this.computerLane = computerLane;
    }
    
    public CreateTerrain.ComputerLane getComputerLane(){
        return computerLane;
    }
}
