using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;


public class CreateTerrain : NetworkBehaviour {

    public TerrainData[] laneSegments;
    public TerrainData base1;
    public TerrainData base2;

    // Use this for initialization
    void Start () {
        int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
        int screenNumber = PlayerPrefs.GetInt("screen", -1);

        GameObject[] chunks = new GameObject[numScreens];
        Vector3 offset = new Vector3(100, 0, 0);

        if (isServer || screenNumber == 0)
        {
            // create the first base
            chunks[0] = Terrain.CreateTerrainGameObject(base1);
        }
        

        //create lane segments
        for (int chunkIndex=1; chunkIndex<numScreens-1; chunkIndex++)
        {
            if (isServer || screenNumber == chunkIndex)
            {
                // should randomly generate where the 0 is between 0->|laneSegments| to get random lane segments 
                chunks[chunkIndex] = Terrain.CreateTerrainGameObject(laneSegments[0]);
                // offset sufficiently
                chunks[chunkIndex].transform.position += offset * chunkIndex;
            }
        }

        if (isServer || screenNumber == (numScreens - 1))
        {
            // create last base
            chunks[numScreens - 1] = Terrain.CreateTerrainGameObject(base2);
            // offset sufficiently
            chunks[numScreens - 1].transform.position += offset * (numScreens - 1);
        }
        
    }
	

	// Update is called once per frame
	void Update () {
	
	}
}
