using UnityEngine;
using System.Collections;

public class GridNode {
    
    public bool walkable;
    public Vector3 worldPoint;
    public int gCost, hCost;
    public int gridX, gridY;
    public GridNode parent;
    
    public GridNode(bool walkable, Vector3 worldPoint, int gridX, int gridY) {
        this.walkable = walkable;
        this.worldPoint = worldPoint;
        this.gridX = gridX;
        this.gridY = gridY;
    }
    
    public int fCost {
        get {
            return gCost + hCost;
        }
    }
}