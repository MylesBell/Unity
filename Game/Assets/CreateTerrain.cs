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
    }

    [System.Serializable]
    public class RequestSceneryMessage : MessageBase {
        public int screenNumber;
        public ComputerLane computerLane;
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
	public int minNumScenery = 100;
	public int maxNumScenery = 200;
    private List<NetworkTreeMessage>[] screenSceneryLeft;
    private List<NetworkTreeMessage>[] screenSceneryRight;
    
    private bool MultipleLanes;

    void Start() {
        //register handlers for messages
        if (isServer) NetworkServer.RegisterHandler(MyMsgType.RequestSceneryCode, onRequestScenery);
        NetworkManager.singleton.client.RegisterHandler(MyMsgType.SendSceneryCode, onRecieverScenery);
        
        Random.seed = (int)Time.time;
        
        int numScreensLeft = PlayerPrefs.GetInt("numberofscreens-left", 0);
        int numScreensRight = PlayerPrefs.GetInt("numberofscreens-right", 0);
        int screenNumber = PlayerPrefs.GetInt("screen", -1);
        
        MultipleLanes = numScreensLeft > 0 && numScreensRight > 0;
        base1Left = MultipleLanes ? base1LeftTunnel : base1LeftNoTunnel;
        base1Right = MultipleLanes ? base1RightTunnel : base1RightNoTunnel;
        base2Left = MultipleLanes ? base2LeftTunnel : base2LeftNoTunnel;
        base2Right = MultipleLanes ? base2RightTunnel : base2RightNoTunnel;
        
        Vector3 chunkOffset = new Vector3(100, 0, 0);
        
        //set up left lane
        if(numScreensLeft > 0) {
		    GenerateTerrain (screenNumber, numScreensLeft, chunkOffset, ComputerLane.LEFT);
            if (isServer) screenSceneryLeft = PopulateScenery (screenNumber, numScreensLeft, chunkOffset, ComputerLane.LEFT);
            else RequestScenery(screenNumber, ComputerLane.LEFT);
        
        }
        
        //set up right lane
        if(numScreensRight > 0) {
		    GenerateTerrain (screenNumber, numScreensRight, chunkOffset, ComputerLane.RIGHT);
            if (isServer) screenSceneryRight = PopulateScenery (screenNumber, numScreensRight, chunkOffset, ComputerLane.RIGHT);
            else RequestScenery(screenNumber, ComputerLane.RIGHT);
        }
        
        if(!isServer) Debug.Log("[ "+ screenNumber + "] MsgType " + MyMsgType.SendSceneryCode);
    }

	void GenerateTerrain(int screenNumber, int numScreens, Vector3 chunkOffset, ComputerLane computerLane) {
        GameObject[] chunks = new GameObject[numScreens];
        Vector3 laneOffset = new Vector3(0,0,(computerLane == ComputerLane.LEFT ? 200 : 0));
		if (isServer || screenNumber == 0)
		{
			// create the first base
			chunks[0] = (GameObject)Instantiate(computerLane == ComputerLane.LEFT ? base1Left : base1Right, laneOffset, Quaternion.identity);
		}
		
		
		//create lane segments
		for (int chunkIndex = 1; chunkIndex < numScreens - 1; chunkIndex++)
		{
			if (isServer || screenNumber == chunkIndex)
			{
				// should randomly generate where the 0 is between 0->|laneSegments| to get random lane segments
				chunks[chunkIndex] = (GameObject)Instantiate(computerLane == ComputerLane.LEFT ? laneSegmentsLeft[0] : laneSegmentsRight[0], chunkOffset * chunkIndex + laneOffset, Quaternion.identity);
                chunks[chunkIndex].GetComponentsInChildren<MeshRenderer>()[0].material = chunkIndex < numScreens / 2? sandMaterial: groundMaterial;
			}
		}
		
		if (isServer || screenNumber == (numScreens - 1))
		{
			// create last base
			chunks[numScreens - 1] = (GameObject)Instantiate(computerLane == ComputerLane.LEFT ? base2Left : base2Right, chunkOffset * (numScreens - 1) + laneOffset, Quaternion.identity);
		}
	}

	List<NetworkTreeMessage>[] PopulateScenery(int screenNumber, int numScreens, Vector3 chunkOffset, ComputerLane computerLane) {
        List<NetworkTreeMessage>[] screenScenery = new List<NetworkTreeMessage>[numScreens];
        Vector3 laneOffset = new Vector3(0,0,(computerLane == ComputerLane.LEFT ? 200 : 0));
        
		for (int i = 0; i < numScreens; i++) {
            int numObjects = Random.Range(minNumScenery,maxNumScenery);
            screenScenery[i] = new List<NetworkTreeMessage>();
            // for (int j = 0; j < numObjects; j++) {
            //     int index = i < numScreens/2? Random.Range(0,2) : Random.Range(2,sceneryObjects.Length);
            //     float z_pos;
            //     do {
            //         z_pos = Random.Range(0, 100) + laneOffset.z;
            //     } while (z_pos >= (computerLane == ComputerLane.LEFT ? Teams.minZLeft : Teams.minZRight) - 2
            //             && z_pos <= (computerLane == ComputerLane.LEFT ? Teams.maxZLeft : Teams.maxZRight) + 2);
            //     Vector3 position = (chunkOffset * i) + new Vector3(Random.Range(0,100),40.0f,z_pos);
            //     RaycastHit terrainLevel;
            //     if(Physics.Raycast(position, -Vector3.up, out terrainLevel, Mathf.Infinity, terrainMask))
            //         position = terrainLevel.point;
            //     Quaternion rotation = Quaternion.Euler(0.0f,Random.Range(0,360),0.0f);
            //     Vector3 scale = new Vector3(Random.Range(0.8f,1.2f), Random.Range(0.8f,1.2f), Random.Range(0.8f,1.2f));

            //     //spawn on server
            //     scenerySpawner(index, position, rotation, scale);
            //     //create a message for the client
            //     //the constructor has to have no parameters though
            //     NetworkTreeMessage msg = new NetworkTreeMessage();
            //     msg.index = index;
            //     msg.position = position;
            //     msg.rotation = rotation;
            //     msg.scale = scale;

            //     screenScenery[i].Add(msg);
            // }
        }
        return screenScenery;
	}

    private void RequestScenery(int screenNumber, ComputerLane computerLane){
        RequestSceneryMessage msg = new RequestSceneryMessage();
        msg.screenNumber = screenNumber;
        msg.computerLane = computerLane;
        NetworkManager.singleton.client.Send(MyMsgType.RequestSceneryCode, msg);
        Debug.Log("Client requested scenery");
    }

    public void onRequestScenery(NetworkMessage netMsg) {
        Debug.Log("[host] Request received.");
        RequestSceneryMessage msg = netMsg.ReadMessage<RequestSceneryMessage>();
        Debug.Log("[host] Request from screen " + msg.screenNumber + " for lane " + msg.computerLane + " recieved");
        //send back the messages
        foreach(NetworkTreeMessage thisTree in (msg.computerLane == ComputerLane.LEFT ?  screenSceneryLeft : screenSceneryRight)[msg.screenNumber]){
            NetworkServer.SendToClient(netMsg.conn.connectionId, MyMsgType.SendSceneryCode, thisTree);
        }
    }

    public void onRecieverScenery(NetworkMessage netMsg) {
        //recieve message and spawn on client
        Debug.Log("Client received message");
        NetworkTreeMessage msg = netMsg.ReadMessage<NetworkTreeMessage>();
        scenerySpawner(msg.index, msg.position, msg.rotation, msg.scale);
    }

    GameObject scenerySpawner(int index, Vector3 position, Quaternion rotation, Vector3 scale){
        GameObject scenery = (GameObject)Instantiate(sceneryObjects[index], position, rotation);
        scenery.transform.localScale = scale;
        return scenery;
    }
}
