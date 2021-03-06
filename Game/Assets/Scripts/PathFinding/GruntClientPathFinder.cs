﻿using UnityEngine;
using UnityEngine.Networking;

public class GruntClientPathFinder : NetworkBehaviour {
    RendererChecker rendererChecker;
    private TeamID teamID;
    private float ForwardMovementTarget = 50;
    private float threshold = 10;
    private NavGridManager navGridManager;
    ComputerLane currentLane;
    private TargetSelect targetSelect;
    
    private Vector3 targetPosition;
    
    [SyncVar] private Vector3 currentTargetPosition;
    
    private bool wasVisible = false;
    private bool recievePaths = true;
    private float minX = 20f;
    private float maxX;
    
    private float nextScreenXPos;
    
    private int screenNumber;
    
    public LayerMask layerMask;
    
    void Start(){
        teamID = gameObject.tag.Contains("red") ? TeamID.red : TeamID.blue;
        if(isServer) {
            targetSelect = GetComponent<TargetSelect>();
        } else {
            rendererChecker = GetComponent<RendererChecker>();
            ForwardMovementTarget *= teamID == TeamID.blue ? 1 : -1;
            navGridManager = GameObject.FindGameObjectsWithTag("terrainSpawner")[0].GetComponent<NavGridManager>();
            currentLane = GraniteNetworkManager.lane;
            maxX = (currentLane == ComputerLane.LEFT ? GraniteNetworkManager.numberOfScreens_left : GraniteNetworkManager.numberOfScreens_right)*CreateTerrain.chunkOffset.x - 25f;
            
            screenNumber = GraniteNetworkManager.screeNumber;
            nextScreenXPos = screenNumber * CreateTerrain.chunkOffset.x + (teamID == TeamID.blue ? 110 : -10);
            nextScreenXPos = Mathf.Clamp(nextScreenXPos, minX, maxX);
            // DebugConsole.Log("Next screen x is " + nextScreenXPos + " and screen is " + screenNumber);
        }
    }
    
    public void InitilizePathFindiding(Vector3 position){
        currentTargetPosition = position;
        wasVisible = false;
        recievePaths = true;
    }
    
    void Update(){
        if(!isServer) {
            if(rendererChecker.visible){
                if(!wasVisible){
                    wasVisible = true;
                    targetPosition = currentTargetPosition;
                    targetPosition.y = GetComponent<BoxCollider>().bounds.size.y/2;
                }
                float distance = Vector3.Distance(targetPosition, transform.position);
                if(distance < threshold){
                    RequestPath();
                }
            } else {
                wasVisible = false;
            }
        }
    }
    
    public void ForceRequest(Vector3 newTargetPosition){
        RpcForceRequest(newTargetPosition);
    }
    
    [ClientRpc]
    public void RpcForceRequest(Vector3 position) {
        if(!isServer && rendererChecker && rendererChecker.visible){
            targetPosition = FindNewTargetPosition(position);
            NavGridManager.RequestLongPath(transform.position, targetPosition, navGridManager.getLongPathGrid(currentLane), OnForcedPathFound);
        }
    }
    public void Panic(){
        RpcPanic();
    }
    
    [ClientRpc]
    public void RpcPanic() {
        if(!isServer &&  rendererChecker && rendererChecker.visible){
            targetPosition = transform.position;
            recievePaths = true;
            // DebugConsole.Log("I am " + teamID + " and panicking at " + transform.position);
        }
    }
    
    private void RequestPath(){
        targetPosition = FindNewTargetPosition(targetPosition);
        if((currentTargetPosition - targetPosition).magnitude > 5f) {
            NavGridManager.RequestLongPath(currentTargetPosition, targetPosition, navGridManager.getLongPathGrid(currentLane), OnPathFound);
        } else {
            currentTargetPosition = targetPosition;
        }
    }
    
    public void OnForcedPathFound(Vector3 start, Vector3 end, Vector3[] newPath, bool pathSuccessful){
        if (pathSuccessful && newPath.Length > 0) {
            PathfindingMessage msg = new PathfindingMessage();
            msg.path = newPath;
            msg.teamID = teamID;
            msg.id = gameObject.GetComponent<Grunt>().GetID();
            msg.screen = screenNumber;
            msg.computerLane = currentLane;
            NetworkManager.singleton.client.Send(MyPathfindingMsg.ReceiveForcedPathCode, msg);
        }
    }
    
    public void OnReceiveForcedPathMessage(PathfindingMessage msg) {
        ComputerLane computerLane = (int)transform.position.z/100 == 0 ? ComputerLane.RIGHT : ComputerLane.LEFT;
        int screenNumber = (int)transform.position.x/(int)CreateTerrain.chunkOffset.x;
        if(screenNumber == msg.screen && computerLane == msg.computerLane){
            targetSelect.AddToQueue(msg.path);
            currentTargetPosition = msg.path[msg.path.Length - 1];
        }
    }
    
    public void OnPathFound(Vector3 start, Vector3 end, Vector3[] newPath, bool pathSuccessful) {
        if (pathSuccessful && newPath.Length > 0) {
            targetPosition = newPath[newPath.Length - 1];
            // DebugConsole.Log("Screen " + screenNumber + " Current position " + transform.position + " Target position " + targetPosition);
            PathfindingMessage msg = new PathfindingMessage();
            msg.path = newPath;
            msg.teamID = teamID;
            msg.id = gameObject.GetComponent<Grunt>().GetID();
            msg.screen = screenNumber;
            msg.computerLane = currentLane;
            NetworkManager.singleton.client.Send(MyPathfindingMsg.ReceivePathCode, msg);
            // DebugConsole.Log("Path found at " + System.DateTime.Now);
        }
    }
    
    public void OnReceivePathMessage(PathfindingMessage msg) {
        ComputerLane computerLane = (int)transform.position.z/100 == 0 ? ComputerLane.RIGHT : ComputerLane.LEFT;
        int screenNumber = (int)transform.position.x/(int)CreateTerrain.chunkOffset.x;
        // DebugConsole.Log("Recieved message from screen " + msg.screen);
        if(recievePaths && screenNumber == msg.screen && computerLane == msg.computerLane) {
            targetSelect.AddToQueue(msg.path);
            currentTargetPosition = msg.path[msg.path.Length - 1];
        }
    }
    
    private Vector3 FindNewTargetPosition(Vector3 input){
        Vector3 position = input;
        position.x = nextScreenXPos;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        int failed = 0 ;
        while(!navGridManager.getLongPathGrid(currentLane).GetGridNodeFromWorldPoint(position).walkable){
            if(failed > 100){
                break;
            } else if(failed > 50){
                position.x -= ForwardMovementTarget/10;
            } else if (failed > 10){
                position.x += ForwardMovementTarget/10;
            } else {
                position.x += ForwardMovementTarget/50;
            }
            position.x = Mathf.Clamp(position.x, minX, maxX);
            failed++;
        }
        // DebugConsole.Log("I am " + teamID + " Desired position " + position);
        return position;
    }
    
    public void StopPaths(){
        recievePaths = false;
    }
    public void StartPaths(){
        recievePaths = true;
    }
}
