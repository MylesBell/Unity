using UnityEngine;
using System.Collections.Generic;

public class LongPathGrid {
    
    public Vector2 gridWorldSize;
    public Vector3 gridWorldCentre;
    public Vector3 gridWorldOrigin;
    public GridNode[,] grid;
    
    int gridSizeX, gridSizeY;
    
    
    public LongPathGrid(Vector3 terrainCentre, Vector2 terrainSize, float nodeRadius, LayerMask unwalkableLayer) {
        this.gridWorldSize = terrainSize;
        this.gridWorldCentre = terrainCentre;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/(nodeRadius*2));
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/(nodeRadius*2));
        grid = new GridNode[gridSizeX,gridSizeY];
        gridWorldOrigin = gridWorldCentre - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;
        
        for (int x = 0; x < gridSizeX; ++x) {
            for (int y = 0; y < gridSizeY; ++y) {
                Vector3 worldPoint = gridWorldOrigin + Vector3.right * (x * (nodeRadius * 2) + nodeRadius)
                        + Vector3.forward * (y * (nodeRadius * 2) + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableLayer));
                grid[x,y] = new GridNode(walkable, worldPoint,x,y);
            }
        }
    }
    
    public int MaxSize {
        get {
            return gridSizeX * gridSizeY;
        }
    }
    
    public GridNode GetGridNodeFromWorldPoint(Vector3 worldPoint) {
        float percentX = Mathf.Abs(gridWorldOrigin.x - worldPoint.x) / gridWorldSize.x;
        float percentY = Mathf.Abs(gridWorldOrigin.z - worldPoint.z)  / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x,y];
    }
    
    public List<GridNode> GetNeighbours(GridNode node) {
        List<GridNode> neighbours = new List<GridNode>();
        
        for (int x = -1; x <= 1; ++x) {
            for (int y = -1; y <= 1; ++y) {
                if (x == 0 && y == 0)
                    continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                
                if (checkX >= 0 && checkX < gridSizeX &&
                    checkY >= 0 && checkY < gridSizeY) {
                        neighbours.Add(grid[checkX,checkY]);
                    }
            }
        }
        return neighbours;
    }

}