using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UnitFactory: MonoBehaviour {

	public GameObject teamsObject;
	int playerCounter;
	private Team blueTeam, redTeam;

	// Use this for initialization
	void Start () {
		playerCounter = 0;
		blueTeam = GameObject.Find ("BlueTeamBase").GetComponent<Team>();
		redTeam = GameObject.Find ("RedTeamBase").GetComponent<Team>();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.P)) {
			ExecuteEvents.Execute<IPlayerJoin> (teamsObject, null, (x,y) => x.PlayerJoin(playerCounter));
			playerCounter++;
		}
		if (Input.GetKeyUp (KeyCode.G)) {
			blueTeam.CreateGrunt();
		}
		if (Input.GetKeyUp (KeyCode.H)) {
			redTeam.CreateGrunt();
		}
	}
}
