using UnityEngine;
using UnityEngine.Networking;

public class GruntClientPathFinder : NetworkBehaviour {
    RendererChecker rendererChecker;
    private TeamID teamID;
    private float ForwardMovementTarget = 50;
    private float threshold = 6;
    private NavGridManager navGridManager;
    private LongPathGrid longPathGrid;
    private TargetSelect targetSelect;
    
    private Vector3 targetPosition;
    
    private bool wasVisible = false;
    private bool recievePaths = true;
    
    void Start(){
        teamID = gameObject.tag.Contains("red") ? TeamID.red : TeamID.blue;
        targetSelect = GetComponent<TargetSelect>();
        if(!isServer){
            rendererChecker = GetComponent<RendererChecker>();
            ForwardMovementTarget *= gameObject.tag.Contains("blue") ? 1 : -1;
            navGridManager = GameObject.FindGameObjectsWithTag("terrainSpawner")[0].GetComponent<NavGridManager>();
            ComputerLane currentLane = (ComputerLane)PlayerPrefs.GetInt("lane");
            longPathGrid = navGridManager.getLongPathGrid(currentLane);
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
            NavGridManager.RequestLongPath(position, targetPosition, longPathGrid, OnForcedPathFound);
        }
    }
    
    private void RequestPath(){
        targetPosition = FindNewTargetPosition(targetPosition);
        NavGridManager.RequestLongPath(transform.position, targetPosition, longPathGrid, OnPathFound);
    }
    
    public void OnForcedPathFound(Vector3[] newPath, bool pathSuccessful){
        if (pathSuccessful && newPath.Length > 0) {
            PathfindingMessage msg = new PathfindingMessage();
            msg.path = newPath;
            msg.teamID = teamID;
            msg.id = gameObject.GetComponent<Grunt>().GetID();
            NetworkManager.singleton.client.Send(MyPathfindingMsg.ReceiveForcedPathCode, msg);
        }
    }
    
    public void OnReceiveForcedPathMessage(PathfindingMessage msg) {
        targetSelect.AddToQueue(msg.path);
    }
    
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
        if (pathSuccessful && newPath.Length > 0) {
            PathfindingMessage msg = new PathfindingMessage();
            msg.path = newPath;
            msg.teamID = teamID;
            msg.id = gameObject.GetComponent<Grunt>().GetID();
            NetworkManager.singleton.client.Send(MyPathfindingMsg.ReceivePathCode, msg);
        }
    }
    
    public void OnReceivePathMessage(PathfindingMessage msg) {
        if(recievePaths) targetSelect.AddToQueue(msg.path);
    }
    
    private Vector3 FindNewTargetPosition(Vector3 input){
        Vector3 position = input + new Vector3(ForwardMovementTarget, 0, 0);
        float radius = ForwardMovementTarget/2f;
        while(Physics.CheckSphere(position, Mathf.Abs(radius))){
            position.x += radius;
        }
        return position;
    }
    
    public void StopPaths(){
        recievePaths = false;
    }
    public void StartPaths(){
        recievePaths = true;
    }
}
