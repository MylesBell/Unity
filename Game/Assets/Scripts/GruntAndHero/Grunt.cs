using UnityEngine;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour, IDestroyableGameObject {

    public Team team;
    [SyncVar] private int id;
    [SyncVar] private bool active = false;

    void Start() {
        gameObject.SetActive(active);
    }
    
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

    public void ResetGameObject(Vector3 spawnPosition, Vector3 desiredPosition) {
        if (isServer) {
            active = true;
            gameObject.GetComponent<Attack>().initiliseAttack();
            gameObject.GetComponent<GruntMovement>().initialiseMovement(spawnPosition);
            //set Health to Max
            gameObject.GetComponent<Health>().InitialiseHealth();
            gameObject.GetComponent<TargetSelect>().InitialiseTargetSelect(team.GetTeamID(), desiredPosition);
            gameObject.SetActive(active);
            CmdSetActiveState(active);
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
        gameObject.SetActive(active);
        CmdSetActiveState(active);
        team.OnGruntDead(gameObject);
    }
    
    public int GetID(){
        return id;
    }
}