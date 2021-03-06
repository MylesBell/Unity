﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public enum TeamID {
	red, blue
}
public class MyPathfindingMsg {
    public static short ReceiveForcedPathCode = MsgType.Highest + 11;
    public static short ReceivePathCode = MsgType.Highest + 12;
}

[System.Serializable]
public class PathfindingMessage : MessageBase {
    public Vector3[] path;
    public int id;
    public TeamID teamID;
    public int screen;
    public ComputerLane computerLane;
}

public class Teams : NetworkBehaviour, IPlayerJoin, IPlayerLeave, IServerDisconnect {

	public Team blueTeam, redTeam;
        
    public int gruntPoolSize;
    public int numberOfGruntsToSpawn;
    public int gruntSpawnInterval;
    public int heroRespawnInterval;

    private float zPositionOffsetRight;
    private float zPositionOffsetLeft;
    public int numberOfChannels;
    public static float maxZRight = 80;
    public static float minZRight = 30;
    public static float maxZLeft = 270;
    public static float minZLeft = 220;
    
    public static float topOffsetRight = 3;
    public static float bottomOffsetRight = 5;
    public static float topOffsetLeft = 3;
    public static float bottomOffsetLeft = 5;

    private bool initialised;
    private bool basesInitialised;

	void Start () {
        if (isServer) {
            initialised = false;
            basesInitialised = false;
            NetworkServer.RegisterHandler(MyPathfindingMsg.ReceivePathCode, OnReceivePathMessage);
            NetworkServer.RegisterHandler(MyPathfindingMsg.ReceiveForcedPathCode, OnReceiveForcedPathMessage);
            zPositionOffsetRight = ((maxZRight-topOffsetRight) - (minZRight+bottomOffsetRight)) / numberOfChannels;
            zPositionOffsetLeft = ((maxZLeft-topOffsetLeft) - (minZLeft+bottomOffsetLeft)) / numberOfChannels;
            int numScreensLeft = GraniteNetworkManager.numberOfScreens_left;
            int numScreensRight = GraniteNetworkManager.numberOfScreens_right;
            bool hasLeftLane = numScreensLeft > 1;
            bool hasRightLane = numScreensRight > 1;
            int blueBaseXPosLeft = 25;
            int blueBaseXPosRight = 25;
            int redBaseXPosLeft = numScreensLeft * 100 - 25;
            int redBaseXPosRight = numScreensRight * 100 - 25;
            blueTeam.Initialise(hasLeftLane, hasRightLane, blueBaseXPosLeft, blueBaseXPosRight, zPositionOffsetLeft, zPositionOffsetRight,numberOfChannels, numberOfGruntsToSpawn, gruntSpawnInterval, gruntPoolSize, heroRespawnInterval);
            redTeam.Initialise(hasLeftLane,hasRightLane, redBaseXPosLeft, redBaseXPosRight, zPositionOffsetLeft, zPositionOffsetRight, numberOfChannels, numberOfGruntsToSpawn, gruntSpawnInterval, gruntPoolSize, heroRespawnInterval);
            gameObject.GetComponent<Towers>().Initialise(numScreensLeft, numScreensRight, blueTeam, redTeam);
        }
    }
    
    void Update() {
        if (isServer) { 
            switch (GameState.gameState) {
                case GameState.State.IDLE:
                    if (!initialised) resetGame();
                    break;
                case GameState.State.PLAYING:
                    if(!basesInitialised) resetBases();
                    break;
                case GameState.State.END:
                    initialised = false;
                    break;
            }
            if(Input.GetKeyDown(KeyCode.K)) ServerDisconnect();
            
            // // uncomment to create test hero
            // if (Input.GetKeyUp(KeyCode.Slash)) redTeam.CreatePlayer("id", "Test Hero");
            
            // if (Input.GetKeyDown(KeyCode.I)) ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.N));
            // if (Input.GetKeyDown(KeyCode.M)) ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.S));
            // if (Input.GetKeyDown(KeyCode.J)) ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.W));
            // if (Input.GetKeyDown(KeyCode.K)) ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.E));
            
            // if (Input.GetKeyUp(KeyCode.I) || Input.GetKeyUp(KeyCode.M) || Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.K)){
            //    ExecuteEvents.Execute<IHeroMovement> (GetHero("id"), null, (x,y) => x.PlayerMovement(MoveDirection.NONE));
            // }      
        }
    }

	public GameObject GetHero(string playerID) {
		GameObject hero;
        if (!blueTeam.TryGetHero(playerID, out hero)) redTeam.TryGetHero(playerID, out hero);
		return hero;
	}
    
    public void OnReceiveForcedPathMessage(NetworkMessage netMsg) {
        PathfindingMessage msg = netMsg.ReadMessage<PathfindingMessage>();
        GameObject grunt;
        if(msg.teamID == TeamID.red) {
            redTeam.TryGetGrunt(msg.id, out grunt);
        } else {
            blueTeam.TryGetGrunt(msg.id, out grunt);
        }
        if(grunt){
            grunt.GetComponent<GruntClientPathFinder>().OnReceiveForcedPathMessage(msg);
        }
    }
    
    public void OnReceivePathMessage(NetworkMessage netMsg) {
        PathfindingMessage msg = netMsg.ReadMessage<PathfindingMessage>();
        // Debug.Log("Recived a path for "  + msg.teamID + " id:" + msg.id);
        GameObject grunt;
        if(msg.teamID == TeamID.red) {
            redTeam.TryGetGrunt(msg.id, out grunt);
        } else {
            blueTeam.TryGetGrunt(msg.id, out grunt);
        }
        if(grunt){
            grunt.GetComponent<GruntClientPathFinder>().OnReceivePathMessage(msg);
        }
        // if(recievePaths) targetSelect.AddToQueue(msg.path);
    }

    private void resetGame() {
        blueTeam.resetTeam();
        redTeam.resetTeam();
        gameObject.GetComponent<Towers>().ResetTowers();
        initialised = true;
    }
    
    private void resetBases(){
        redTeam.resetBases();
        blueTeam.resetBases();
        basesInitialised = true;
    }

	#region IPlayerJoin implementation
	public void PlayerJoin (string playerID, string playerName, int playerClass, string gameCode) {
        
        if(GraniteNetworkManager.game_code == gameCode) {
            int blueHeroes = blueTeam.GetNumberOfHeros();
            int redHeroes = redTeam.GetNumberOfHeros();
            if (blueHeroes < redHeroes) {
                blueTeam.CreatePlayer(playerID, playerName, playerClass);
            } else {
                redTeam.CreatePlayer(playerID, playerName, playerClass);
            }
        } else {
	        SocketIOOutgoingEvents.PlayerJoinFailInvalidGameCode (playerID);
        }
	}
    #endregion

    #region IPlayerLeave implementation
    public void PlayerLeave(string playerID)
    {
        GameObject hero;
        if (blueTeam.TryGetHero(playerID, out hero))
            blueTeam.RemovePlayer(playerID);
        else
            redTeam.RemovePlayer(playerID);
            
    }

    public void ServerDisconnect()
    {
        List<string> keysBlue = new List<string>(blueTeam.playerDict.Keys);
        List<string> keysRed = new List<string>(redTeam.playerDict.Keys);
        foreach(string playerID in keysBlue){
            blueTeam.RemovePlayer(playerID);
        }
        foreach(string playerID in keysRed){
            redTeam.RemovePlayer(playerID);
        }
    }
    #endregion

}
