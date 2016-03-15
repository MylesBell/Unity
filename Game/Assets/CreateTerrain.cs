﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
    
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
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
    
    public GameObject[] laneSegmentsLeft;
    public GameObject base1LeftNoTunnel;
    public GameObject base2LeftNoTunnel;
    public GameObject base1LeftTunnel;
    public GameObject base2LeftTunnel;
    
    public GameObject[] laneSegmentsRight;
    public GameObject base1RightNoTunnel;
    public GameObject base2RightNoTunnel;
    public GameObject base1RightTunnel;
    public GameObject base2RightTunnel;
    private GameObject base1Left;
    private GameObject base2Left;
    private GameObject base1Right;
    private GameObject base2Right;
    public Material groundMaterial;
    public Material sandMaterial;
	public GameObject[] sceneryObjects;
	public LayerMask terrainMask;
	public int minNumScenerySides = 100;
	public int maxNumScenerySides = 200;
	private int minNumSceneryMain = 10;
	private int maxNumSceneryMain = 20;
    public static Vector3 chunkOffset = new Vector3(100, 0, 0);
    private List<NetworkTreeMessage>[] screenSceneryLeft;
    private List<NetworkTreeMessage>[] screenSceneryRight;
    private int objectsToRecieve = 0;
    private int screensRequested = 0;
    private int objectsRecieved = 0;
    private bool generatedPathfinder;
    
    private bool MultipleLanes;
    
    private object objectsRecievedLock = new object();

    void Start() {
        //register handlers for messages
        if (isServer) NetworkServer.RegisterHandler(MyMsgType.RequestSceneryCode, onRequestScenery);
        NetworkManager.singleton.client.RegisterHandler(MyMsgType.SendSceneryCode, onRecieverScenery);
        NetworkManager.singleton.client.RegisterHandler(MyMsgType.SceneryInfoCode, onRecieverSceneryInfo);
        
        Random.seed = (int)Time.time;
        int numScreensLeft = GraniteNetworkManager.numberOfScreens_left;
        int numScreensRight = GraniteNetworkManager.numberOfScreens_right;
        int screenNumber = GraniteNetworkManager.screeNumber;
        
        MultipleLanes = numScreensLeft > 0 && numScreensRight > 0;
        base1Left = MultipleLanes ? base1LeftTunnel : base1LeftNoTunnel;
        base1Right = MultipleLanes ? base1RightTunnel : base1RightNoTunnel;
        base2Left = MultipleLanes ? base2LeftTunnel : base2LeftNoTunnel;
        base2Right = MultipleLanes ? base2RightTunnel : base2RightNoTunnel;
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
                if(objectsToRecieve == objectsRecieved || GameState.instance.networkGameState == GameState.State.PLAYING){
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
            if(computerLane == ComputerLane.LEFT) screenSceneryLeft = PopulateScenery (numScreens, computerLane);
            else                                  screenSceneryRight = PopulateScenery (numScreens, computerLane);
            GenerateLongPathGridServer(numScreens,computerLane);
        } else {
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
            RequestScenery(screenNumbers.ToArray(), computerLane);
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
				chunks[chunkIndex] = (GameObject)Instantiate(computerLane == ComputerLane.LEFT ? laneSegmentsLeft[0] : laneSegmentsRight[0], chunkOffset * chunkIndex + laneOffset, Quaternion.identity);
                chunks[chunkIndex].GetComponentsInChildren<MeshRenderer>()[0].material = chunkIndex < numScreens / 2? sandMaterial: groundMaterial;
			}
		}
		
		if (isServer || screenNumber == (numScreens - 1)) {
			// create last base
			chunks[numScreens - 1] = (GameObject)Instantiate(computerLane == ComputerLane.LEFT ? base2Left : base2Right, chunkOffset * (numScreens - 1) + laneOffset, Quaternion.identity);
		}
	}

	List<NetworkTreeMessage>[] PopulateScenery(int numScreens, ComputerLane computerLane) {
        List<NetworkTreeMessage>[] screenScenery = new List<NetworkTreeMessage>[numScreens];
        Vector3 laneOffset = new Vector3(0,0,(computerLane == ComputerLane.LEFT ? 200 : 0));
        
		for (int i = 0; i < numScreens; i++) {
            int numObjectsOnSides = Random.Range(minNumScenerySides, maxNumScenerySides);
            int numObjectsOnMainTerrain = Random.Range(minNumSceneryMain, maxNumSceneryMain);
            screenScenery[i] = new List<NetworkTreeMessage>();
            screenScenery[i].AddRange(GenerateTerrainPart(false, numObjectsOnSides, numScreens, i, computerLane, laneOffset));
            screenScenery[i].AddRange(GenerateTerrainPart(true, numObjectsOnMainTerrain, numScreens, i, computerLane, laneOffset));
        }
        return screenScenery;
	}
    
    private List<NetworkTreeMessage> GenerateTerrainPart(bool generatingForMainTerrain, int numObjects, int numScreens, int screenNumber, ComputerLane computerLane, Vector3 laneOffset){
        List<NetworkTreeMessage> screenScenery = new List<NetworkTreeMessage>();
        for (int j = 0; j < numObjects; j++) {
            int index = screenNumber < numScreens/2? Random.Range(0,3) : Random.Range(3,sceneryObjects.Length);
            Vector3 position = GetNewPosition(generatingForMainTerrain, screenNumber, numScreens, chunkOffset*screenNumber, computerLane, laneOffset);
            RaycastHit terrainLevel;
            if(Physics.Raycast(position, -Vector3.up, out terrainLevel, Mathf.Infinity, terrainMask))
                position = terrainLevel.point;
            Quaternion rotation = Quaternion.Euler(0.0f,Random.Range(0,360),0.0f);
            Vector3 scale = new Vector3(Random.Range(0.8f,1.2f), Random.Range(0.8f,1.2f), Random.Range(0.8f,1.2f));

            //spawn on server
            scenerySpawner(index, position, rotation, scale);
            //create a message for the client
            //the constructor has to have no parameters though
            NetworkTreeMessage msg = new NetworkTreeMessage();
            msg.index = index;
            msg.position = position;
            msg.rotation = rotation;
            msg.scale = scale;

            screenScenery.Add(msg);
        }
        return screenScenery;
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
    
    Vector3 GetNewPosition(bool generatingForMainTerrain, int screenNumber, int numScreens, Vector3 offset, ComputerLane computerLane, Vector3 laneOffset){
        Vector3 position = offset;
        float z_min =  (computerLane == ComputerLane.LEFT ? Teams.minZLeft : Teams.minZRight) - 2;
        float z_max = (computerLane == ComputerLane.LEFT ? Teams.maxZLeft : Teams.maxZRight) + 2;
        float z_pos, x_pos;
        if(generatingForMainTerrain){
            z_pos = z_min + Random.Range(0, z_max - z_min);
        } else {
            do {
                z_pos = Random.Range(0, 100) + laneOffset.z;
            } while (z_pos >= z_min && z_pos <= z_max);
        }
        
        //Don't put obstacles in the tunnels
        //first check if this z_pos is on the tunnel side or not
        bool tunnelSide = (computerLane == ComputerLane.LEFT && z_pos < z_min)
                          || (computerLane == ComputerLane.RIGHT && z_pos > z_max);
        if(MultipleLanes && (screenNumber == 0 || screenNumber == numScreens - 1) && tunnelSide) {
            //Find a point that is not between A and B so that it's not in the tunnel
            float A, B;
            if(screenNumber == 0){
                A = 56;
                B = 76;
            } else {
                A = 24;
                B = 44;
            }
            do {
                x_pos = Random.Range(0, 100);
            } while (x_pos >= A && x_pos <= B);
        } else {
            bool nearBase = (computerLane == ComputerLane.LEFT && z_pos < z_max)|| (computerLane == ComputerLane.RIGHT && z_pos > z_min);
            if(screenNumber == 0 && nearBase){
                x_pos = Random.Range(50,100);
            } else if(screenNumber == numScreens - 1 && nearBase){
                x_pos = Random.Range(0,50);
            } else {
                x_pos = Random.Range(0,100);
            }
        }
        position += new Vector3(x_pos, 40.0f,z_pos);
        return position;
    }

    private void RequestScenery(int[] screenNumbers, ComputerLane computerLane){
        RequestSceneryMessage msg = new RequestSceneryMessage();
        msg.screenNumbers = screenNumbers;
        msg.computerLane = computerLane;
        NetworkManager.singleton.client.Send(MyMsgType.RequestSceneryCode, msg);
        Debug.Log("Client requested scenery");
    }

    public void onRequestScenery(NetworkMessage netMsg) {
        Debug.Log("[host] Request received.");
        RequestSceneryMessage msg = netMsg.ReadMessage<RequestSceneryMessage>();
        Debug.Log("[host] Request for screens " + string.Join(",", System.Array.ConvertAll(msg.screenNumbers, x => x.ToString())) + " for lane " + msg.computerLane + " recieved");
        //Send info about scenery objects
        SceneryInfoMessage simMsg = new SceneryInfoMessage();
        simMsg.numberOfElements = 0;
        foreach(int screenNumber in msg.screenNumbers) {
            simMsg.numberOfElements += (msg.computerLane == ComputerLane.LEFT ?  screenSceneryLeft : screenSceneryRight)[screenNumber].Count;
        }
        NetworkServer.SendToClient(netMsg.conn.connectionId, MyMsgType.SceneryInfoCode, simMsg);
        //send back the messages
        foreach(int screenNumber in msg.screenNumbers) {
            foreach(NetworkTreeMessage thisTree in (msg.computerLane == ComputerLane.LEFT ?  screenSceneryLeft : screenSceneryRight)[screenNumber]){
                NetworkServer.SendToClient(netMsg.conn.connectionId, MyMsgType.SendSceneryCode, thisTree);
            }
        }
    }

    public void onRecieverScenery(NetworkMessage netMsg) {
        //recieve message and spawn on client
        Debug.Log("Client received message");
        NetworkTreeMessage msg = netMsg.ReadMessage<NetworkTreeMessage>();
        scenerySpawner(msg.index, msg.position, msg.rotation, msg.scale);
        lock(objectsRecievedLock){
            objectsRecieved++;
        }
    }
    
    public void onRecieverSceneryInfo(NetworkMessage netMsg) {
        SceneryInfoMessage msg = netMsg.ReadMessage<SceneryInfoMessage>();
        objectsToRecieve = msg.numberOfElements;
    }

    GameObject scenerySpawner(int index, Vector3 position, Quaternion rotation, Vector3 scale){
        GameObject scenery = (GameObject)Instantiate(sceneryObjects[index], position, rotation);
        scenery.transform.localScale = scale;
        return scenery;
    }
}
