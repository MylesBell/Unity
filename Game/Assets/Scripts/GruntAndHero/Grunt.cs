﻿using UnityEngine;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour, IDestroyableGameObject {

    public Team team;
    [SyncVar] private int id;
    private bool active = false;
    private ComputerLane computerLane;

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
    
    public void ResetGameObject(Vector3 spawnPosition, ComputerLane computerLane) {
        if (isServer) {
            active = true;
            Vector3 adjusted_spawnLocation = gameObject.GetComponent<GruntMovement>().AdjustToTerrain(spawnPosition);
            gameObject.GetComponent<Attack>().initiliseAttack();
            gameObject.GetComponent<GruntMovement>().initialiseMovement(adjusted_spawnLocation);
            //set Health to Max
            gameObject.GetComponent<Health>().InitialiseHealth(computerLane);
            gameObject.GetComponent<TargetSelect>().InitialiseTargetSelect(team.GetTeamID(), spawnPosition);
            gameObject.GetComponent<SynchronisedMovement>().ResetMovement(team.teamID,adjusted_spawnLocation);
            CmdSetActiveState(active,adjusted_spawnLocation);
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