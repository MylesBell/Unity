using UnityEngine;
using System.Collections.Generic;

public class Navigator : MonoBehaviour {
    
    public Transform target; //For testing
    
    LongPathGrid longPathGrid;
    bool initialised = false;
     
    public void InitialiseNavigator(LongPathGrid longGrid) {
        if (!initialised) {
            this.longPathGrid = longGrid;
            initialised = true;
        }
    }
    
    public void CalculatePath(Vector3 startPosition, Vector3 targetPosition) {
        GridNode startNode = longPathGrid.GetGridNodeFromWorldPoint(startPosition);
        GridNode targetNode = longPathGrid.GetGridNodeFromWorldPoint(targetPosition);
        
        Heap<GridNode> openSet = new Heap<GridNode>(longPathGrid.MaxSize);
        HashSet<GridNode> closedSet = new HashSet<GridNode>();
        
        openSet.Add(startNode);
        
        while(openSet.Count > 0) {
            GridNode currentNode = openSet.RemoveFirst();

            closedSet.Add(currentNode);
            
            if (currentNode == targetNode) {
                RetracePath(startNode, targetNode);
                return;
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
    
    void RetracePath(GridNode startNode, GridNode targetNode) {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = targetNode;
        
        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
        path.Reverse();
        
        longPathGrid.path = path;
        
    }
    
    int GetDistance(GridNode nodeA, GridNode nodeB) {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
    
    void Update() {
        if (initialised)
            CalculatePath(transform.position, target.position);
    }
}