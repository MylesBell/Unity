﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Grunt : NetworkBehaviour, IDestroyableGameObject {

    private Team team;
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

    public void ResetGameObject(Vector3 spawnPosition, Vector3 desiredPosition, float channelOffset) {
        if (isServer) {
            active = true;
            gameObject.GetComponent<Attack>().initiliseAttack();
            gameObject.GetComponent<Movement>().initialiseMovement(spawnPosition);
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
            gameObject.GetComponent<TargetSelect>().InitialiseTargetSelect (team.GetTeamID(), desiredPosition, channelOffset);
            gameObject.SetActive(active);
            CmdSetActiveState(active);
        }
	}

    void Update() {
        if (isServer && GameState.gameState != GameState.State.PLAYING) DisableGameObject(); //kill grunts at the end
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
}