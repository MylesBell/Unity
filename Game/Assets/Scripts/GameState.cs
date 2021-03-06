﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {
    public const string IDLE_STRING = "Preparing...";
    public enum State { IDLE, PLAYING, END };
    
    public static bool allowPlayersJoin;

    public static State gameState;
    public static TeamID winningTeam;
    
    public State networkGameState;
    public static GameState instance;
    private static Team[] teams;
    
    public MusicScreenController musicScreenController;

    // Use this for initialization
    void Start () {
        teams = gameObject.GetComponents<Team>();
        instance = this;
        allowPlayersJoin = false;
        gameState = State.IDLE;
        SetText(IDLE_STRING);
    }
	
	// Update is called once per frame
	void Update () {
        if (isServer) {
            if (Input.GetKeyUp(KeyCode.W)) {
                allowPlayersJoin = true;
            }
            if (Input.GetKeyUp(KeyCode.S)) {
                if(gameState == State.IDLE) StartCoroutine(StartGame());
            }

            if (Input.GetKeyUp(KeyCode.E)) {
                if(gameState == State.PLAYING) endGame(TeamID.blue);
            }

            if (Input.GetKeyUp(KeyCode.Q)) {
                if(gameState == State.END) {
                    musicScreenController.StopMusic();
                    changeGameState(State.IDLE);
                }
            }
        }
    }
    
    private IEnumerator StartGame(){
        musicScreenController.StartMusic(3);
        for(int i = 3; i > 0; i--){
            RpcSetText(""+i);
            yield return new WaitForSeconds(1f);
        }
        changeGameState(State.PLAYING);
    }

    public static void changeGameState(State state) {
        gameState = state;
        instance.RpcStateAndText(gameState, winningTeam);
		SocketIOOutgoingEvents.GameStateChange (state);
    }

	public static void endGame(TeamID winner) {
        allowPlayersJoin = false;
		winningTeam = winner;
		changeGameState(State.END);
        SocketIOOutgoingEvents.SendPlayerStats(teams);
		Debug.Log(winner + " won!\n");
	}
    
    [ClientRpc]
    public void RpcSetText(string text){
        SetText(text);
    }

    [ClientRpc]
    public void RpcStateAndText(GameState.State networkGameState, TeamID winner) {
        this.networkGameState = networkGameState;
        string text;
        switch(networkGameState){
            case State.IDLE:
                text = IDLE_STRING;
                break;
            case State.END:
                winningTeam = winner;
                //Blue is cowboys
                text = (winner == TeamID.blue ? "Cowboys" : "Vikings") + " won!";
                break;
            default:
                text = "";
                break;
        }
        SetText(text);
    }

    public void SetText(string text) {
        Camera.main.transform.FindChild("GameStateText").GetComponent<TextMesh>().text = text;

    }
}
