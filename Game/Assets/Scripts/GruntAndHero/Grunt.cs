using UnityEngine;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour, IDestroyableGameObject {

    public Team team;
    [SyncVar] private int id;
    private bool active = false;

    void Start() {
        gameObject.SetActive(active);
    }
    
    public void InitialiseGameObject(Team team) {
        if (isServer) {
            this.team = team;
            gameObject.SetActive(active);
            CmdSetActiveState(active, transform.position);
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
            gameObject.GetComponent<SynchronisedMovement>().ResetMovement(team.teamID,spawnPosition);
            CmdSetActiveState(active,spawnPosition);
            gameObject.SetActive(active);
        }
	}

    void Update() {
        if (isServer && GameState.gameState == GameState.State.IDLE) DisableGameObject(); //kill grunts at restart
    }

    [Command]
    public void CmdSetActiveState(bool active, Vector3 spawnPosition) {
        RpcSetActive(active, spawnPosition);
    }

    [ClientRpc]
    public void RpcSetActive(bool active, Vector3 spawnPosition) {
        transform.position = spawnPosition;
        gameObject.SetActive(active);
    }

    public void DisableGameObject() {
        active = false;
        gameObject.SetActive(active);
        CmdSetActiveState(active, transform.position);
        team.OnGruntDead(gameObject);
    }
    
    public int GetID(){
        return id;
    }
}