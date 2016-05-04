using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public enum ComputerLane {LEFT, RIGHT}

public class CreateTerrain : NetworkBehaviour
{
    public class MyMsgType
    {
        public static short RequestSceneryCode = MsgType.Highest + 1;
        public static short SendSceneryCode = MsgType.Highest + 2;
        public static short SceneryInfoCode = MsgType.Highest + 3;
    }

    [System.Serializable]
    public class RequestSceneryMessage : MessageBase {
        public int[] screenNumbers;
        public ComputerLane computerLane;
    }

    [System.Serializable]
    public class SceneryInfoMessage : MessageBase {
        public int numberOfElements;
    }

    [System.Serializable]
    public class NetworkTreeMessage : MessageBase {
        public int index;
        public float snowLevel;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
    
    public GameObject base1Left;
    public GameObject base2Left;
    public GameObject base1Right;
    public GameObject base2Right;
    public GameObject[] laneSegmentsCowboyLeft;
    public GameObject middleLaneLeft;
    public GameObject[] laneSegmentsVikingLeft;
    public GameObject[] laneSegmentsCowboyRight;
    public GameObject middleLaneRight;
    public GameObject[] laneSegmentsVikingRight;
    public GameObject snowObject;
	public LayerMask terrainMask;
    public static Vector3 chunkOffset = new Vector3(100, 0, 0);
    private int objectsToRecieve = 0;
    private int screensRequested = 0;
    private int objectsRecieved = 0;
    private bool generatedPathfinder;
    
    private bool MultipleLanes;
    
    private object objectsRecievedLock = new object();

    void Start() {
        
        UnityEngine.Random.seed = DateTime.Now.Millisecond;
        int numScreensLeft = GraniteNetworkManager.numberOfScreens_left;
        int numScreensRight = GraniteNetworkManager.numberOfScreens_right;
        int screenNumber = GraniteNetworkManager.screeNumber;
        
        MultipleLanes = numScreensLeft > 0 && numScreensRight > 0;
        generatedPathfinder = false;
        
        //set up left lane
        if((isServer || GraniteNetworkManager.lane == ComputerLane.LEFT) && numScreensLeft > 0) GenerateComputerLane(screenNumber, numScreensLeft, ComputerLane.LEFT);
        
        //set up right lane
        if((isServer || GraniteNetworkManager.lane == ComputerLane.RIGHT) && numScreensRight > 0) GenerateComputerLane(screenNumber, numScreensRight, ComputerLane.RIGHT);
    }
    
    void Update(){
        //only generate the long path grid ONCE on the client, after it has recieved all of the scenery or game has started
        if(!isServer && !generatedPathfinder && objectsToRecieve > 0) {
            lock(objectsRecievedLock){
                if(GameState.instance.networkGameState == GameState.State.PLAYING){
                    ComputerLane computerLane = GraniteNetworkManager.lane;
                    int screenNumber = GraniteNetworkManager.screeNumber;
                    GenerateLongPathGrid(screenNumber, screensRequested, computerLane);
                    generatedPathfinder = true;
                }
            }
        }
    }
    
    void GenerateComputerLane(int screenNumber, int numScreens, ComputerLane computerLane){
        if (isServer) {
            GenerateTerrain(screenNumber, numScreens, computerLane);
            GenerateLongPathGridServer(numScreens,computerLane);
        } else {
            //Request scenry for the current screen and the neighbouring screens if possible
            List<int> screenNumbers = new List<int>();
            if(screenNumber > 0) {
                GenerateTerrain(screenNumber-1, numScreens, computerLane);
                screenNumbers.Add(screenNumber-1);
            }
            
            GenerateTerrain(screenNumber, numScreens, computerLane);
            screenNumbers.Add(screenNumber);

            if(screenNumber + 1 < numScreens) {
                GenerateTerrain(screenNumber+1, numScreens, computerLane);
                screenNumbers.Add(screenNumber+1);
            }
            screensRequested = screenNumbers.Count;
        }
    }

	void GenerateTerrain(int screenNumber, int numScreens, ComputerLane computerLane) {
        GameObject[] chunks = new GameObject[numScreens];
        Vector3 laneOffset = new Vector3(0,0,(computerLane == ComputerLane.LEFT ? 200 : 0));
		if (isServer || screenNumber == 0) {
			// create the first base
			chunks[0] = (GameObject)Instantiate(computerLane == ComputerLane.LEFT ? base1Left : base1Right, laneOffset, Quaternion.identity);
		}
		
		
		//create lane segments
		for (int chunkIndex = 1; chunkIndex < numScreens - 1; chunkIndex++) {
			if (isServer || screenNumber == chunkIndex) {
				// should randomly generate where the 0 is between 0->|laneSegments| to get random lane segments
				chunks[chunkIndex] = GetTerrainPrefab(computerLane, numScreens, chunkIndex, laneOffset);
                if (chunkIndex < numScreens / 2) {
                } else if (chunkIndex == numScreens / 2){
                    Color sand = new Color(1.0f,0.85f,0.55f);
                    chunks[chunkIndex].GetComponentsInChildren<MeshRenderer>()[0].material.SetColor("_SnowColor",sand);
                    chunks[chunkIndex].GetComponentsInChildren<MeshRenderer>()[0].material.SetFloat("_Snow",0.35f);
                } else {
                    float level = (chunkIndex - (numScreens / 2)) / (float)(numScreens- (numScreens/2));
                    CreateSnow(chunks[chunkIndex], level);
                }
			}
		}
		
		if (isServer || screenNumber == (numScreens - 1)) {
			// create last base
			chunks[numScreens - 1] = (GameObject)Instantiate(computerLane == ComputerLane.LEFT ? base2Left : base2Right, chunkOffset * (numScreens - 1) + laneOffset, Quaternion.identity);
            CreateSnow(chunks[numScreens - 1], 1.0f);    
            
		}
	}
    
    GameObject GetTerrainPrefab(ComputerLane computerLane, int numScreens, int chunkIndex, Vector3 laneOffset) {
        int terrainIndex = GetTerrainIndex(numScreens, chunkIndex);
        GameObject terrain;
        if (computerLane == ComputerLane.LEFT) {
            if (chunkIndex < numScreens / 2) {
                terrain = (GameObject)Instantiate(laneSegmentsCowboyLeft[terrainIndex],
                    chunkOffset * chunkIndex + laneOffset, Quaternion.identity);
            } else if(chunkIndex == numScreens / 2) {
                terrain = (GameObject)Instantiate(middleLaneLeft,
                    chunkOffset * chunkIndex + laneOffset, Quaternion.identity);
            }
            else{
                terrain = (GameObject)Instantiate(laneSegmentsVikingLeft[terrainIndex],
                    chunkOffset * chunkIndex + laneOffset, Quaternion.identity);
            }
        } else {
            if (chunkIndex < numScreens / 2) {
                terrain = (GameObject)Instantiate(laneSegmentsCowboyRight[terrainIndex],
                    chunkOffset * chunkIndex + laneOffset, Quaternion.identity);
            } else if(chunkIndex == numScreens / 2) {
                terrain = (GameObject)Instantiate(middleLaneRight,
                    chunkOffset * chunkIndex + laneOffset, Quaternion.identity);
            }
            else{
                terrain = (GameObject)Instantiate(laneSegmentsVikingRight[terrainIndex],
                    chunkOffset * chunkIndex + laneOffset, Quaternion.identity);
            }
        }
        return terrain;
    }
    
    void CreateSnow(GameObject chunk, float snowLevel) {
        foreach (MeshRenderer renderer in chunk.GetComponentsInChildren<MeshRenderer>()) {
            renderer.material.SetFloat("_Snow", snowLevel);
        }
        
        Vector3 position = chunk.transform.position + new Vector3(50,100,50);
        GameObject snow = (GameObject) Instantiate(snowObject, position, Quaternion.identity);
        snow.GetComponent<ParticleSystem>().emissionRate = snowLevel * 1000;
    }
    
    int GetTerrainIndex(int numScreens, int chunkIndex) {
        // Calculating whether to have a tunnel screen or not
        int terrainIndex;
        if (chunkIndex < (numScreens/2))
            // Less than halfway point so count from start
            terrainIndex = chunkIndex % 3;
        else if (chunkIndex == (numScreens/2))
            terrainIndex = 1;
        else
            // Over halfway point so count from end
            terrainIndex = (numScreens - 1 - chunkIndex) % 3;
        return terrainIndex;
    }

    void GenerateLongPathGrid(int screenNumber, int numScreens, ComputerLane computerLane) {
        float xCentre = screenNumber * chunkOffset.x + chunkOffset.x / 2;
        Vector3 gridCentre = new Vector3(xCentre,0,(computerLane == ComputerLane.LEFT ? 250 : 50));
        GetComponent<NavGridManager>().CreateLongPathGrid(gridCentre, new Vector2(numScreens * chunkOffset.x,100), computerLane);

    }
    
    void GenerateLongPathGridServer(int numScreens, ComputerLane computerLane) {
        float xCentre = numScreens * chunkOffset.x / 2;
        Vector3 gridCentre = new Vector3(xCentre,0,(computerLane == ComputerLane.LEFT ? 250 : 50));
        GetComponent<NavGridManager>().CreateLongPathGrid(gridCentre, new Vector2(numScreens * chunkOffset.x,100), computerLane);

    }
}
