using UnityEngine;
using System;
using System.Collections.Generic;

public class NavGridManager : MonoBehaviour {
    
    public Navigator[] navigators;
    private int nodeRadius = 1;
    public LayerMask unwalkableLayer;
    public bool displayGridGizmos = false;
    
    static NavGridManager instance;
    
    LongPathGrid leftGrid,rightGrid;
    int gridSizeX, gridSizeY;
    
    LongPathNavigation longPathNavigation;
    Queue<LongPathRequest> longPathRequestQueue = new Queue<LongPathRequest>();
    LongPathRequest currentLongPathRequest;
    bool isProcessingLongPath = false;
    
    struct LongPathRequest {
        public Vector3 pathStart;
        public Vector3 pathTarget;
        public LongPathGrid longPathGrid;
        public Action<Vector3[], bool> callback;
        
        public LongPathRequest(Vector3 start, Vector3 target, LongPathGrid grid, Action<Vector3[], bool> call){
            pathStart = start;
            pathTarget = target;
            longPathGrid = grid;
            callback = call;
        }
    }
    
    void Awake() {
        instance = this;
        longPathNavigation = GetComponent<LongPathNavigation>();
        longPathNavigation.AttachNavGridManager(this);
    }
    
    public void CreateLongPathGrid(Vector3 gridCentre, Vector2 gridSize, ComputerLane computerLane) {
        if (computerLane == ComputerLane.LEFT)
            leftGrid = new LongPathGrid(gridCentre, gridSize, nodeRadius, unwalkableLayer);
        else {
            rightGrid = new LongPathGrid(gridCentre, gridSize, nodeRadius, unwalkableLayer);
            foreach (Navigator navigator in navigators)
                navigator.InitialiseNavigator(rightGrid);
        }
    }
    
    public LongPathGrid getLongPathGrid(ComputerLane computerLane) {
        return computerLane == ComputerLane.LEFT ? leftGrid : rightGrid;
    }
    
    public static void RequestLongPath(Vector3 pathStart, Vector3 pathTarget, LongPathGrid longPathGrid, Action<Vector3[], bool> callback) {
        LongPathRequest newRequest = new LongPathRequest(pathStart, pathTarget, longPathGrid, callback);
        instance.longPathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }
    
    void TryProcessNext() {
        if (!isProcessingLongPath && longPathRequestQueue.Count > 0) {
			currentLongPathRequest = longPathRequestQueue.Dequeue();
			isProcessingLongPath = true;
			longPathNavigation.StartFindPath(currentLongPathRequest.pathStart, currentLongPathRequest.pathTarget, 
                    currentLongPathRequest.longPathGrid);
		}
    }
    
    public void FinishProcessingLongPath(Vector3[] path, bool success) {
        currentLongPathRequest.callback(path,success);
        isProcessingLongPath = false;
        TryProcessNext();
    }
    
    void OnDrawGizmos() {
        
        if (leftGrid != null && displayGridGizmos) {
            foreach (GridNode node in leftGrid.grid) {
                Gizmos.color = node.walkable? Color.white : Color.red;
                Gizmos.DrawCube(node.worldPoint, Vector3.one * ((nodeRadius * 2) - 0.1f));
            }
        }
        
        if (rightGrid != null && displayGridGizmos) {
            foreach (GridNode node in rightGrid.grid) {
                Gizmos.color = node.walkable? Color.white : Color.red;
                Gizmos.DrawCube(node.worldPoint, Vector3.one * ((nodeRadius * 2) - 0.1f));
            }
        }
    }

}