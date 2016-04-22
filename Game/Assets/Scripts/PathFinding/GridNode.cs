using UnityEngine;

public class GridNode : IHeapItem<GridNode>{
    
    public bool walkable;
    public Vector3 worldPoint;
    public int gCost, hCost;
    public int gridX, gridY;
    public GridNode parent;
    int heapIndex;
    
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

    public int HeapIndex {
        get {
            return heapIndex;
        }

        set {
            heapIndex = value;
        }
    }

    public int CompareTo(GridNode other) {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0) {
            compare = hCost.CompareTo(other.hCost);
        }
        return -compare;
    }
}