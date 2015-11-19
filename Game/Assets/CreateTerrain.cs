using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class CreateTerrain : NetworkBehaviour
{

    public GameObject[] laneSegments;
    public GameObject base1;
    public GameObject base2;

    // Use this for initialization
    void Start()
    {
        int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
        int screenNumber = PlayerPrefs.GetInt("screen", -1);

        GameObject[] chunks = new GameObject[numScreens];
        Vector3 offset = new Vector3(100, 0, 0);

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

    // Update is called once per frame
    void Update()
    {

    }
}
