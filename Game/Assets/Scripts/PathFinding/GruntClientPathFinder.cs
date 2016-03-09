using UnityEngine;
using UnityEngine.Networking;

public class GruntClientPathFinder : NetworkBehaviour {
    RendererChecker rendererChecker;
    private TeamID teamID;
    private float ForwardMovementTarget = 50;
    private float threshold = 1;
    private NavGridManager navGridManager;
    private LongPathGrid longPathGrid;
    private TargetSelect targetSelect;
    
    private Vector3 targetPosition;
    
    private bool wasVisible = false;
    private bool recievePaths = true;
    private float minX = 20f;
    private float maxX;
    
    private float nextScreenXPos;
    
    private int screenNumber;
    
    public LayerMask layerMask;
    
    void Start(){
        teamID = gameObject.tag.Contains("red") ? TeamID.red : TeamID.blue;
        targetSelect = GetComponent<TargetSelect>();
        if(!isServer){
            rendererChecker = GetComponent<RendererChecker>();
            ForwardMovementTarget *= teamID == TeamID.blue ? 1 : -1;
            navGridManager = GameObject.FindGameObjectsWithTag("terrainSpawner")[0].GetComponent<NavGridManager>();
            ComputerLane currentLane = GraniteNetworkManager.lane;
            longPathGrid = navGridManager.getLongPathGrid(currentLane);
            maxX = (currentLane == ComputerLane.LEFT ? GraniteNetworkManager.numberOfScreens_left : GraniteNetworkManager.numberOfScreens_right)*CreateTerrain.chunkOffset.x - 20f;
            
            screenNumber = GraniteNetworkManager.screeNumber;
            nextScreenXPos = screenNumber * CreateTerrain.chunkOffset.x + (teamID == TeamID.blue ? 110 : -10);
            nextScreenXPos = Mathf.Clamp(nextScreenXPos, minX, maxX);
            DebugConsole.Log("Next screen x is " + nextScreenXPos + " and screen is " + screenNumber);
        }
    }
    
    void Update(){
        if(!isServer) {
            if(rendererChecker.visible){
                if(!wasVisible){
                    wasVisible = true;
                    targetPosition = transform.position;
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
        if(!isServer && rendererChecker.visible){
            targetPosition = FindNewTargetPosition(position);
            NavGridManager.RequestLongPath(transform.position, targetPosition, longPathGrid, OnForcedPathFound);
        }
    }
    public void Panic(){
        RpcPanic();
        DebugConsole.Log("Panic is on");
    }
    
    [ClientRpc]
    public void RpcPanic() {
        if(!isServer && rendererChecker.visible){
            targetPosition = transform.position;
            recievePaths = true;
            DebugConsole.Log("I am " + teamID + " and panicking");
        }
    }
    
    private void RequestPath(){
        DebugConsole.Log("I am " + teamID + " requesting path");
        targetPosition = FindNewTargetPosition(targetPosition);
        NavGridManager.RequestLongPath(transform.position, targetPosition, longPathGrid, OnPathFound);
    }
    
    public void OnForcedPathFound(Vector3[] newPath, bool pathSuccessful){
        if (pathSuccessful && newPath.Length > 0) {
            PathfindingMessage msg = new PathfindingMessage();
            msg.path = newPath;
            msg.teamID = teamID;
            msg.id = gameObject.GetComponent<Grunt>().GetID();
            msg.screen = screenNumber;
            NetworkManager.singleton.client.Send(MyPathfindingMsg.ReceiveForcedPathCode, msg);
        }
    }
    
    public void OnReceiveForcedPathMessage(PathfindingMessage msg) {
        targetSelect.AddToQueue(msg.path);
    }
    
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
        if (pathSuccessful && newPath.Length > 0) {
            targetPosition = newPath[newPath.Length - 1];
            DebugConsole.Log("Screen " + screenNumber + " Current position " + transform.position + " Target position " + targetPosition);
            PathfindingMessage msg = new PathfindingMessage();
            msg.path = newPath;
            msg.teamID = teamID;
            msg.id = gameObject.GetComponent<Grunt>().GetID();
            msg.screen = screenNumber;
            NetworkManager.singleton.client.Send(MyPathfindingMsg.ReceivePathCode, msg);
        }
    }
    
    public void OnReceivePathMessage(PathfindingMessage msg) {
        DebugConsole.Log("Recieved message from screen " + msg.screen);
         foreach (Vector3 v in msg.path)
            {
            DebugConsole.Log( v.ToString() );
            }
        if(recievePaths) targetSelect.AddToQueue(msg.path);
    }
    
    private Vector3 FindNewTargetPosition(Vector3 input){
        Vector3 position = input;
        position.x = nextScreenXPos;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        int failed = 0 ;
        while(!longPathGrid.GetGridNodeFromWorldPoint(position).walkable){
            if(failed > 100){
                break;
            } else if(failed > 50){
                position.x -= ForwardMovementTarget/10;
            } else if (failed > 10){
                position.x += ForwardMovementTarget/50;
            } else {
                position.x += ForwardMovementTarget/10;
            }
            position.x = Mathf.Clamp(position.x, minX, maxX);
            failed++;
        }
        DebugConsole.Log("I am " + teamID + " Desired position " + position);
        return position;
    }
    
    public void StopPaths(){
        recievePaths = false;
    }
    public void StartPaths(){
        recievePaths = true;
    }
}
