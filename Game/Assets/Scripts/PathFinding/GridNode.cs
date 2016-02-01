using UnityEngine;
using System.Collections;

public class GridNode {
    
    public bool walkable;
    public Vector3 worldPoint;
    
    public GridNode(bool walkable, Vector3 worldPoint) {
        this.walkable = walkable;
        this.worldPoint = worldPoint;
    }
}