using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class UnitFactory: NetworkBehaviour {
	
	public GameObject teamsObject, redBase, blueBase, heroPrefab, blueGruntPrefab, redGruntPrefab;
	int playerCounter;
	private Team blueTeam, redTeam;
	private GameObject redTower, blueTower;

	// Use this for initialization
	void Start () {
		if (isServer) {
			int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
			redTower = (GameObject)Instantiate(redBase, new Vector3(50,2,50), Quaternion.identity);
			blueTower = (GameObject) Instantiate(blueBase, new Vector3(numScreens * 100 - 50, 2, 50), Quaternion.identity);
			NetworkServer.Spawn(redTower);
			NetworkServer.Spawn(blueTower);
			blueTeam = blueTower.GetComponent<Team>();
			redTeam = redTower.GetComponent<Team>();
			playerCounter = 0;
		}
	}

	// Update is called once per frame
	void Update () {
		if (isServer) {
			if (Input.GetKeyUp (KeyCode.P)) {
				ExecuteEvents.Execute<IPlayerJoin> (teamsObject, null, (x,y) => x.PlayerJoin (playerCounter));
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
		Vector3 spawnLocation = GetSpawnLocation (teamID);
		Vector3 targetLocation = GetTargetLocation (teamID);
		GameObject gruntPrefab = teamID == TeamID.blue ? blueGruntPrefab : redGruntPrefab;
		GameObject gruntObject = (GameObject) Instantiate (gruntPrefab, spawnLocation, Quaternion.identity);
		gruntObject.GetComponent<Grunt> ().InitialiseGrunt (teamID, targetLocation);
		NetworkServer.Spawn (gruntObject);

		return gruntObject;
	}

	public GameObject CreateHero(TeamID teamID, int playerID) {
		Vector3 spawnLocation = GetSpawnLocation (teamID);
		Vector3 targetLocation = GetTargetLocation (teamID);
		GameObject heroObject = Instantiate (heroPrefab, spawnLocation, Quaternion.identity) as GameObject;
		Hero hero = heroObject.GetComponent<Hero> ();
		hero.InitialiseHero (teamID, targetLocation);
		NetworkServer.Spawn (heroObject);
		return heroObject;
	}

	Vector3 GetSpawnLocation (TeamID teamID) {
		Vector3 spawnLocation = Vector3.zero;
		if (teamID == TeamID.red)
			spawnLocation = redTower.transform.position + new Vector3(4,0,0);
		else
			spawnLocation = blueTower.transform.position - new Vector3(4,0,0);
		return spawnLocation;
	}

	Vector3 GetTargetLocation (TeamID teamID) {
		Vector3 targetLocation = Vector3.zero;
		if (teamID == TeamID.red) 
			targetLocation = blueTower.transform.position - new Vector3 (4, 0, 0);
		else
			targetLocation = redTower.transform.position + new Vector3 (4, 0, 0);
		return targetLocation;
	}
}
