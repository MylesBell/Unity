using UnityEngine;
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
	public int minNumScenerySides = 100;
	public int maxNumScenerySides = 200;
	private int minNumSceneryMain = 10;
	private int maxNumSceneryMain = 20;
    private List<NetworkTreeMessage>[] screenSceneryLeft;
    private List<NetworkTreeMessage>[] screenSceneryRight;
    
    private bool MultipleLanes;

    void Start() {
        //register handlers for messages
        if (isServer) NetworkServer.RegisterHandler(MyMsgType.RequestSceneryCode, onRequestScenery);
        NetworkManager.singleton.client.RegisterHandler(MyMsgType.SendSceneryCode, onRecieverScenery);
        
        Random.seed = (int)Time.time;
        int lane = PlayerPrefs.GetInt("lane", 0);
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
        if((isServer || lane == 0) && numScreensLeft > 0) GenerateComputerLane(lane, screenNumber, numScreensLeft, chunkOffset, ComputerLane.LEFT);
        
        //set up right lane
        if((isServer || lane == 1) && numScreensRight > 0) GenerateComputerLane(lane, screenNumber, numScreensRight, chunkOffset, ComputerLane.RIGHT);
        
    }
    
    void GenerateComputerLane(int lane, int screenNumber, int numScreens, Vector3 chunkOffset, ComputerLane computerLane){
        int screensGenerated = 0;
        if (isServer) {
            GenerateTerrain(screenNumber, numScreens, chunkOffset, computerLane);
            if(computerLane == ComputerLane.LEFT) screenSceneryLeft = PopulateScenery (numScreens, chunkOffset, computerLane);
            else                                  screenSceneryRight = PopulateScenery (numScreens, chunkOffset, computerLane);
            GenerateLongPathGridServer(numScreens, chunkOffset,computerLane);
        } else {
            if(screenNumber > 0) {
                GenerateClientScreen(screenNumber-1, numScreens, chunkOffset, computerLane);
                screensGenerated++;
            }
            GenerateClientScreen(screenNumber, numScreens, chunkOffset, computerLane);
            screensGenerated++;
            if(screenNumber + 1 < numScreens) {
                GenerateClientScreen(screenNumber+1, numScreens, chunkOffset, computerLane);
                screensGenerated++;
            }
            GenerateLongPathGrid(screenNumber, screensGenerated, chunkOffset,computerLane);
        }
    }
    
    void GenerateClientScreen(int screenNumber, int numScreens, Vector3 chunkOffset, ComputerLane computerLane){
        GenerateTerrain(screenNumber, numScreens, chunkOffset, computerLane);
        RequestScenery(screenNumber, computerLane);
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

	List<NetworkTreeMessage>[] PopulateScenery(int numScreens, Vector3 chunkOffset, ComputerLane computerLane) {
        List<NetworkTreeMessage>[] screenScenery = new List<NetworkTreeMessage>[numScreens];
        Vector3 laneOffset = new Vector3(0,0,(computerLane == ComputerLane.LEFT ? 200 : 0));
        
		for (int i = 0; i < numScreens; i++) {
            int numObjectsOnSides = Random.Range(minNumScenerySides, maxNumScenerySides);
            int numObjectsOnMainTerrain = Random.Range(minNumSceneryMain, maxNumSceneryMain);
            screenScenery[i] = new List<NetworkTreeMessage>();
            screenScenery[i].AddRange(GenerateTerrainPart(false, numObjectsOnSides, numScreens, i, chunkOffset, computerLane, laneOffset));
            screenScenery[i].AddRange(GenerateTerrainPart(true, numObjectsOnMainTerrain, numScreens, i, chunkOffset, computerLane, laneOffset));
            
        }
        return screenScenery;
	}
    
    private List<NetworkTreeMessage> GenerateTerrainPart(bool generatingForMainTerrain, int numObjects, int numScreens, int screenNumber, Vector3 chunkOffset, ComputerLane computerLane, Vector3 laneOffset){
        List<NetworkTreeMessage> screenScenery = new List<NetworkTreeMessage>();
        for (int j = 0; j < numObjects; j++) {
            int index = screenNumber < numScreens/2? Random.Range(0,2) : Random.Range(2,sceneryObjects.Length);
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

    void GenerateLongPathGrid(int screenNumber, int numScreens, Vector3 chunkOffset, ComputerLane computerLane) {
        float xCentre = screenNumber * chunkOffset.x + chunkOffset.x / 2;
        Vector3 gridCentre = new Vector3(xCentre,0,(computerLane == ComputerLane.LEFT ? 250 : 50));
        
        GetComponent<NavGridManager>().CreateLongPathGrid(gridCentre, new Vector2(numScreens * chunkOffset.x,100), computerLane);

    }
    
    void GenerateLongPathGridServer(int numScreens, Vector3 chunkOffset, ComputerLane computerLane) {
        float xCentre = numScreens * chunkOffset.x / 2;
        Vector3 gridCentre = new Vector3(xCentre,0,(computerLane == ComputerLane.LEFT ? 250 : 50));
        
        GetComponent<NavGridManager>().CreateLongPathGrid(gridCentre, new Vector2(numScreens * chunkOffset.x,100), computerLane);

    }
    
    Vector3 GetNewPosition(bool generatingForMainTerrain, int screenNumber, int numScreens, Vector3 chunkOffset, ComputerLane computerLane, Vector3 laneOffset){
        Vector3 position = chunkOffset;
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
            x_pos = Random.Range(0,100);
        }
        position += new Vector3(x_pos, 40.0f,z_pos);
        return position;
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
