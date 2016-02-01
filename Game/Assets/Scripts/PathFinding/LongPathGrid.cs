using UnityEngine;
using System.Collections;

public class LongPathGrid {
    
    public Vector2 gridWorldSize;
    public Vector3 gridWorldCentre;
    public GridNode[,] grid;
    
    int gridSizeX, gridSizeY;
    
    
    public LongPathGrid(Vector3 terrainCentre, Vector2 terrainSize, float nodeRadius, LayerMask unwalkableLayer) {
        this.gridWorldSize = terrainSize;
        this.gridWorldCentre = terrainCentre;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/(nodeRadius*2));
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/(nodeRadius*2));
        grid = new GridNode[gridSizeX,gridSizeY];
        Vector3 origin = gridWorldCentre - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;
        
        for (int x = 0; x < gridSizeX; ++x) {
            for (int y = 0; y < gridSizeY; ++y) {
                Vector3 worldPoint = origin + Vector3.right * (x * (nodeRadius * 2) + nodeRadius)
                        + Vector3.forward * (y * (nodeRadius * 2) + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableLayer));
                grid[x,y] = new GridNode(walkable, worldPoint);
            }
        }
    }

}