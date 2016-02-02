using UnityEngine;

public class NavGridManager : MonoBehaviour {
    
    public Navigator navigator;
    public float nodeRadius = 1.0f;
    public LayerMask unwalkableLayer;
    public bool onlyDrawNavPath = false;
    
    LongPathGrid leftGrid,rightGrid;
    int gridSizeX, gridSizeY;
    
    public void CreateLongPathGrid(Vector3 gridCentre, Vector2 gridSize, ComputerLane computerLane) {
        if (computerLane == ComputerLane.LEFT)
            leftGrid = new LongPathGrid(gridCentre, gridSize, nodeRadius, unwalkableLayer);
        else {
            rightGrid = new LongPathGrid(gridCentre, gridSize, nodeRadius, unwalkableLayer);
            navigator.InitialiseNavigator(rightGrid);
        }
    }
    
    public LongPathGrid getLongPathGrid(ComputerLane computerLane) {
        return computerLane == ComputerLane.LEFT ? leftGrid : rightGrid;
    }
    
    void OnDrawGizmos() {
        
        if (onlyDrawNavPath) {
            if (leftGrid.path != null) {
                foreach (GridNode node in leftGrid.path) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(node.worldPoint, Vector3.one * ((nodeRadius * 2) - 0.1f));
                }
            }
            
            if (rightGrid.path != null) {
                foreach (GridNode node in rightGrid.path) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(node.worldPoint, Vector3.one * ((nodeRadius * 2) - 0.1f));
                }
            }
        } else { 
            if (leftGrid != null) {
                foreach (GridNode node in leftGrid.grid) {
                    Gizmos.color = node.walkable? Color.white : Color.red;
                    if (leftGrid.path != null && leftGrid.path.Contains(node))
                        Gizmos.color = Color.black;
                    Gizmos.DrawCube(node.worldPoint, Vector3.one * ((nodeRadius * 2) - 0.1f));
                }
            }
            
            if (rightGrid != null) {
                foreach (GridNode node in rightGrid.grid) {
                    Gizmos.color = node.walkable? Color.white : Color.red;
                    if (rightGrid.path != null && rightGrid.path.Contains(node))
                        Gizmos.color = Color.black;
                    Gizmos.DrawCube(node.worldPoint, Vector3.one * ((nodeRadius * 2) - 0.1f));
                }
            }
        }
    }

}