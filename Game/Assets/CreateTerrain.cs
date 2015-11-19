using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class CreateTerrain : NetworkBehaviour
{

    public GameObject[] laneSegments;
    public GameObject base1;
    public GameObject base2;
	public GameObject[] sceneryObjects;
	public LayerMask terrainMask;
	public int minNumScenery = 100;
	public int maxNumScenery = 200;

    void Start() {
        int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
        int screenNumber = PlayerPrefs.GetInt("screen", -1);

        GameObject[] chunks = new GameObject[numScreens];
        Vector3 offset = new Vector3(100, 0, 0);

		GenerateTerrain (screenNumber, numScreens, chunks, offset);

		PopulateScenery (screenNumber, numScreens, offset);

    }

	void GenerateTerrain(int screenNumber, int numScreens, GameObject[] chunks, Vector3 offset) {
		if (isServer || screenNumber == 0)
		{
			// create the first base
			chunks[0] = (GameObject)Instantiate(base1);
		}
		
		
		//create lane segments
		for (int chunkIndex = 1; chunkIndex < numScreens - 1; chunkIndex++)
		{
			if (isServer || screenNumber == chunkIndex)
			{
				// should randomly generate where the 0 is between 0->|laneSegments| to get random lane segments 
				chunks[chunkIndex] = (GameObject)Instantiate(laneSegments[0], offset * chunkIndex, Quaternion.identity);
			}
		}
		
		if (isServer || screenNumber == (numScreens - 1))
		{
			// create last base
			chunks[numScreens - 1] = (GameObject)Instantiate(base2, offset * (numScreens - 1), Quaternion.identity);
		}
	}

	void PopulateScenery(int screenNumber, int numScreens, Vector3 offset) {

		for (int i = 0; i < numScreens; i++) {
			if (isServer || screenNumber == i) {

				int numObjects = Random.Range(minNumScenery,maxNumScenery);

				for (int j = 0; j < numObjects; j++) {

					int index = Random.Range(0,sceneryObjects.Length);
					float z_pos;
					do {
						z_pos = Random.Range(0,100);
					} while (z_pos >= 35 && z_pos <=65);
					Vector3 location = (offset * i) + new Vector3(Random.Range(0,100),20.0f,z_pos);
					RaycastHit terrainLevel;
					if(Physics.Raycast(location, -Vector3.up, out terrainLevel, Mathf.Infinity, terrainMask))
						location = terrainLevel.point;
					Quaternion rotation = Quaternion.Euler(0.0f,Random.Range(0,360),0.0f);
					Vector3 scale = new Vector3(Random.Range(0.8f,1.2f), Random.Range(0.8f,1.2f), Random.Range(0.8f,1.2f));

					GameObject scenery = (GameObject)Instantiate(sceneryObjects[index],location, rotation);
					scenery.transform.localScale = scale;
				}
			}
		}

	}

}
