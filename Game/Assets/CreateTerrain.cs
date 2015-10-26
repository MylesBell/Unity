using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CreateTerrain : MonoBehaviour {

    public int numScreens = 3;
    public TerrainData[] laneSegments;
    public TerrainData base1;
    public TerrainData base2;

    // Use this for initialization
    void Start () {
        GameObject[] chunks = new GameObject[numScreens];
        Vector3 offset = new Vector3(100, 0, 0);

        // create the first base
        chunks[0] = Terrain.CreateTerrainGameObject(base1);

        //create lane segments
        for (int chunkIndex=1; chunkIndex<numScreens-1; chunkIndex++)
        {
            // should randomly generate where the 0 is between 0->|laneSegments| to get random lane segments 
            chunks[chunkIndex] = Terrain.CreateTerrainGameObject(laneSegments[0]);
            // offset sufficiently
            for (int j=0; j<chunkIndex; j++)
            {
                chunks[chunkIndex].transform.position += offset;
            }
        }

        // create last base
        chunks[numScreens - 1] = Terrain.CreateTerrainGameObject(base2);
        // offset sufficiently
        for(int i=0; i<numScreens-1; i++)
        {
            chunks[numScreens - 1].transform.position += offset;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
