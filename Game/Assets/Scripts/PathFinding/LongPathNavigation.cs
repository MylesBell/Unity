using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LongPathNavigation : MonoBehaviour{
    
    NavGridManager navGridManager;
    
    public void AttachNavGridManager(NavGridManager manager) {
        navGridManager = manager;
    }
    
    public void StartFindPath(Vector3 startPosition, Vector3 targetPosition, LongPathGrid longPathGrid) {
        StartCoroutine(CalculatePath(startPosition,targetPosition, longPathGrid));
    }
    
    IEnumerator CalculatePath(Vector3 startPosition, Vector3 targetPosition, LongPathGrid longPathGrid) {
        
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        
        GridNode startNode = longPathGrid.GetGridNodeFromWorldPoint(startPosition);
        GridNode targetNode = longPathGrid.GetGridNodeFromWorldPoint(targetPosition);
        
        if (targetNode.walkable) {
            Heap<GridNode> openSet = new Heap<GridNode>(longPathGrid.MaxSize);
            HashSet<GridNode> closedSet = new HashSet<GridNode>();
            
            openSet.Add(startNode);
            
            while(openSet.Count > 0) {
                GridNode currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);
                
                if (currentNode == targetNode) {
                    pathSuccess = true;
                    break;
                }
                    
                foreach (GridNode neighbour in longPathGrid.GetNeighbours(currentNode)) {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) {
                        continue;
                    }
                    
                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;
                        
                        if(!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
            waypoints = RetracePath(startNode, targetNode);
        navGridManager.FinishProcessingLongPath(startPosition, targetPosition, waypoints,pathSuccess);
    }
    
    Vector3[] RetracePath(GridNode startNode, GridNode targetNode) {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = targetNode;
        
        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
		Vector3[] waypoints = SimplifyPath(path);
		Array.Reverse(waypoints);
		return waypoints;

	}

	Vector3[] SimplifyPath(List<GridNode> path) {
		List<Vector3> waypoints = new List<Vector3>();
		Vector2 directionOld = Vector2.zero;

		for (int i = 1; i < path.Count; i ++) {
			Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX,path[i-1].gridY - path[i].gridY);
			if (directionNew != directionOld) {
				if(waypoints.Count > 0 && waypoints[waypoints.Count - 1] != path[i-1].worldPoint) {
                    waypoints.Add(path[i-1].worldPoint);
                }
				waypoints.Add(path[i].worldPoint);
			}
			directionOld = directionNew;
		}
        
		return waypoints.ToArray();
	}
    
    int GetDistance(GridNode nodeA, GridNode nodeB) {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
}