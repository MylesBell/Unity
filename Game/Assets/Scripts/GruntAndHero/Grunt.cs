using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Grunt : NetworkBehaviour, IDisableGO {

	private TargetSelect targetSelect;
    private Team team;
    [SyncVar] private bool active = false;

    void Start() {
        gameObject.SetActive(active);
    }
    
    public void InitialiseGrunt(Team team, TeamID teamIDInput, Vector3 spawnPosition, Vector3 desiredPosition, float channelOffset) {
        if (isServer) {
            active = true;
            this.team = team;
            gameObject.GetComponent<Attack>().initiliseAttack();
            gameObject.GetComponent<Movement>().initialiseMovement(spawnPosition);
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (teamIDInput, desiredPosition, channelOffset);
            gameObject.SetActive(active);
            CmdSetActiveState(active);
        }
	}

    void Update() {
        if (isServer && GameState.gameState != GameState.State.PLAYING) disableGameObject(); //kill grunts at the end
    }

    [Command]
    public void CmdSetActiveState(bool active) {
        RpcSetActive(active);
    }

    [ClientRpc]
    public void RpcSetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void disableGameObject() {
        active = false;
        gameObject.SetActive(active);
        CmdSetActiveState(active);
        team.OnGruntDead(gameObject);
    }
}