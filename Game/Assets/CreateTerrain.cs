using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

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
    }

    [System.Serializable]
    public class NetworkTreeMessage : MessageBase {
        public int index;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public GameObject[] laneSegments;
    public GameObject base1;
    public GameObject base2;
	public GameObject[] sceneryObjects;
	public LayerMask terrainMask;
	public int minNumScenery = 100;
	public int maxNumScenery = 200;
    private List<NetworkTreeMessage>[] screenScenery;

    void Start() {
        //register handlers for messages
        if (isServer) NetworkServer.RegisterHandler(MyMsgType.RequestSceneryCode, onRequestScenery);
        NetworkManager.singleton.client.RegisterHandler(MyMsgType.SendSceneryCode, onRecieverScenery);
        int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
        int screenNumber = PlayerPrefs.GetInt("screen", -1);

        GameObject[] chunks = new GameObject[numScreens];
        Vector3 offset = new Vector3(100, 0, 0);

		GenerateTerrain (screenNumber, numScreens, chunks, offset);
		if (isServer) PopulateScenery (screenNumber, numScreens, offset);
        else RequestScenery(screenNumber);
        if(!isServer) Debug.Log("[ "+ screenNumber + "] MsgType " + MyMsgType.SendSceneryCode);
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
        screenScenery = new List<NetworkTreeMessage>[numScreens];

		for (int i = 0; i < numScreens; i++) {
            int numObjects = Random.Range(minNumScenery,maxNumScenery);
            screenScenery[i] = new List<NetworkTreeMessage>();
            for (int j = 0; j < numObjects; j++) {

                int index = Random.Range(0,sceneryObjects.Length);
                float z_pos;
                do {
                    z_pos = Random.Range(0, 100);
                } while (z_pos >= (Teams.minZ - 5) && z_pos <= Teams.maxZ);
                Vector3 position = (offset * i) + new Vector3(Random.Range(0,100),20.0f,z_pos);
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

                screenScenery[i].Add(msg);
            }
        }
	}

    private void RequestScenery(int screenNumber){
        RequestSceneryMessage msg = new RequestSceneryMessage();
        msg.screenNumber = screenNumber;
        NetworkManager.singleton.client.Send(MyMsgType.RequestSceneryCode, msg);
        Debug.Log("Client requested scenery");
    }

    public void onRequestScenery(NetworkMessage netMsg) {
        Debug.Log("[host] Request received.");
        RequestSceneryMessage msg = netMsg.ReadMessage<RequestSceneryMessage>();
        Debug.Log("[host] Request from screen " + msg.screenNumber + " Recieved");
        //send back the messages
        foreach(NetworkTreeMessage thisTree in screenScenery[msg.screenNumber]){
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
