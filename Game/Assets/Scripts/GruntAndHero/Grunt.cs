using UnityEngine;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour, IDestroyableGameObject {

    public Team team;
    [SyncVar] private int id;
    private bool active = false;
    private ComputerLane computerLane;
    
    public void InitialiseGameObject(Team team) {
        if (isServer) {
            this.team = team;
            gameObject.SetActive(active);
            CmdSetActiveState(active);
        }
	}
    public void SetID(int id){
        this.id = id;
    }
    
    public void ResetGameObject(Vector3 spawnPosition, ComputerLane computerLane) {
        if (isServer) {
            active = true;
            // Vector3 adjusted_spawnLocation = gameObject.GetComponent<GruntMovement>().AdjustToTerrain(spawnPosition);
            Vector3 rotation = team.GetTeamID() == TeamID.blue ? new Vector3(0,90,0) : new Vector3(0,270,0);
            RpcResetGameObject(spawnPosition, rotation, computerLane);
        }
	}
    
    [ClientRpc]
    public void RpcResetGameObject(Vector3 spawnPosition, Vector3 rotation, ComputerLane computerLane){
        active = true;
        if(isServer) gameObject.GetComponent<Attack>().initiliseAttack();
        gameObject.GetComponent<GruntMovement>().initialiseMovement(spawnPosition);
        gameObject.GetComponent<SynchronisedMovement>().ResetMovement(spawnPosition, rotation);
        if(isServer) gameObject.GetComponent<TargetSelect>().InitialiseTargetSelect(team.GetTeamID(), spawnPosition);
        //set Health to Max
        gameObject.GetComponent<Health>().InitialiseHealth(computerLane);
        gameObject.GetComponent<GruntClientPathFinder>().InitilizePathFindiding(spawnPosition);
        gameObject.SetActive(active);
        for(int i = 0; i < transform.childCount; i++){
            if(transform.GetChild(i).name.Contains("DamageText")){
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void Update() {
        if (isServer && GameState.gameState == GameState.State.IDLE) DisableGameObject(); //kill grunts at restart
    }

    [Command]
    public void CmdSetActiveState(bool active) {
        RpcSetActive(active);
    }

    [ClientRpc]
    public void RpcSetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void DisableGameObject() {
        active = false;
        CmdSetActiveState(active);
        team.OnGruntDead(gameObject);
    }
    
    public int GetID(){
        return id;
    }
}