using UnityEngine;

public class NavGridGenerator : MonoBehaviour {
    
    public LongPathGrid leftGrid,rightGrid;
    public float nodeRadius = 1.0f;
    public LayerMask unwalkableLayer;
    
    int gridSizeX, gridSizeY;
    
    
    public void CreateLongPathGrid(Vector3 gridCentre, Vector2 gridSize, ComputerLane computerLane) {
        if (computerLane == ComputerLane.LEFT)
            leftGrid = new LongPathGrid(gridCentre, gridSize, nodeRadius, unwalkableLayer);
        else
            rightGrid = new LongPathGrid(gridCentre, gridSize, nodeRadius, unwalkableLayer);
    }
    
    void OnDrawGizmos() {
        
        if (leftGrid != null) {
            foreach (GridNode node in leftGrid.grid) {
                Gizmos.color = node.walkable? Color.white : Color.red;
                Gizmos.DrawCube(node.worldPoint, Vector3.one * ((nodeRadius * 2) - 0.1f));
            }
        }
        
        if (rightGrid != null) {
            foreach (GridNode node in rightGrid.grid) {
                Gizmos.color = node.walkable? Color.white : Color.red;
                Gizmos.DrawCube(node.worldPoint, Vector3.one * ((nodeRadius * 2) - 0.1f));
            }
        }
    }

}