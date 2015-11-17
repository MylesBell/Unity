using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class UnitFactory: NetworkBehaviour {
	
	public GameObject teamsObject, redBasePrefab, blueBasePrefab, blueHeroPrefab, redHeroPrefab, blueGruntPrefab, redGruntPrefab;
	private GameObject blueBase, redBase;
	public float channelSeparation;
	private Vector3 blueBaseUpChannelStart, blueBaseDownChannelStart, redBaseUpChannelStart, redBaseDownChannelStart;
	private Vector3 channelOffset;
	private int playerCounter;

	// Use this for initialization
	void Start () {
		if (isServer) {
			playerCounter = 0;
			CreateBases();
			SetChannelStarts();
		}
	}

	private void CreateBases(){
		int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
		blueBase = (GameObject)Instantiate(blueBasePrefab, new Vector3(50,2,50), Quaternion.identity);
		redBase = (GameObject) Instantiate(redBasePrefab, new Vector3(numScreens * 100 - 50, 2, 50), Quaternion.identity);
		NetworkServer.Spawn(blueBase);
		NetworkServer.Spawn(redBase);
	}

	private void SetChannelStarts(){
		Vector3 blueBasePosition = blueBase.transform.position;
		Vector3 redBasePosition = redBase.transform.position;
		Vector3 baseDistance = redBasePosition - blueBasePosition;

		/*float xOffset = Mathf.Sqrt (Mathf.Abs(Mathf.Pow(channelSeparation,2)/
		                            (1+Mathf.Pow(baseDistance.y / baseDistance.x,2))));
		float zOffset = Mathf.Sqrt (Mathf.Abs(Mathf.Pow(channelSeparation,2) + Mathf.Pow(xOffset,2)));
		blueBaseUpChannelStart = blueBasePosition + new Vector3 (-xOffset,0,zOffset);
		blueBaseDownChannelStart = blueBasePosition + new Vector3 (xOffset,0,-zOffset);
		redBaseUpChannelStart = redBasePosition + new Vector3 (-xOffset,0,zOffset);
		redBaseDownChannelStart = redBasePosition + new Vector3 (xOffset,0,-zOffset);*/

		blueBaseUpChannelStart = blueBasePosition + new Vector3 (channelSeparation,0,channelSeparation);
		blueBaseDownChannelStart = blueBasePosition + new Vector3 (channelSeparation,0,-channelSeparation);
		redBaseUpChannelStart = redBasePosition + new Vector3 (-channelSeparation,0,channelSeparation);
		redBaseDownChannelStart = redBasePosition + new Vector3 (-channelSeparation,0,-channelSeparation);
		channelOffset = new Vector3(channelSeparation, 0, 0);
	}

	// Update is called once per frame
	void Update () {
		if (isServer) {
			if (Input.GetKeyUp (KeyCode.P)) {
				ExecuteEvents.Execute<IPlayerJoin> (teamsObject, null, (x,y) => x.PlayerJoin (playerCounter.ToString()));
				playerCounter++;
			}
			if (Input.GetKeyUp (KeyCode.G)) {
				CreateGrunt (TeamID.blue);
			}
			if (Input.GetKeyUp (KeyCode.H)) {
				CreateGrunt (TeamID.red);
			}
		}
	}

	public GameObject CreateGrunt(TeamID teamID) {
		GameObject gruntPrefab = teamID == TeamID.blue ? blueGruntPrefab : redGruntPrefab;
		Vector3 spawnLocation = GetSpawnLocation (teamID);
		Debug.Log(spawnLocation);
		Channel channel = getChannel();
		Vector3 channelTarget = GetLaneTarget (teamID, channel);
		GameObject gruntObject = Instantiate (gruntPrefab, spawnLocation, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (gruntObject);
		Grunt grunt = gruntObject.GetComponent<Grunt> ();
		grunt.InitialiseGrunt (teamID, channel, channelTarget, channelOffset);
		return gruntObject;
	}

	public GameObject CreateHero(TeamID teamID) {
		GameObject heroPrefab = teamID == TeamID.blue ? blueHeroPrefab : redHeroPrefab;
		Vector3 spawnLocation = GetSpawnLocation (teamID);
		Channel channel = getChannel();
		Vector3 channelTarget = GetLaneTarget (teamID, channel);
		GameObject heroObject = Instantiate (heroPrefab, spawnLocation, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (heroObject);
		Hero hero = heroObject.GetComponent<Hero> ();
		hero.InitialiseHero (teamID, channel, channelTarget, channelOffset);
		return heroObject;
	}

	private Vector3 GetSpawnLocation (TeamID teamID) {
		Vector3 spawnLocation = Vector3.zero;
		if (teamID == TeamID.blue)
			spawnLocation = blueBase.transform.position + new Vector3(4,0,0);
		else
			spawnLocation = redBase.transform.position - new Vector3(4,0,0);
		return spawnLocation;
	}

	private Channel getChannel(){
		int randomNumber = Random.Range(0,2);
		if (randomNumber == 0) {
			return Channel.up;
		}
		return Channel.down;
	}
	
	private Vector3 GetLaneTarget (TeamID teamID, Channel channel) {
		Vector3 targetLocation = Vector3.zero;
		if (teamID == TeamID.blue) {
			if (channel == Channel.up){
				targetLocation = blueBaseUpChannelStart;
			}else{
				targetLocation = blueBaseDownChannelStart;
			}
		} else {
			if (channel == Channel.up){
				targetLocation = redBaseUpChannelStart;
			}else{
				targetLocation = redBaseDownChannelStart;
			}
		}
		return targetLocation;
	}
}
