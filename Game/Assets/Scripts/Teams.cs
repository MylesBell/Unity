using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TeamID {
	red, blue
}

public enum Direction {
	up, down
}

public class Teams : MonoBehaviour, IPlayerJoin {

	Team blueTeam, redTeam;
	int numPlayers;
	Dictionary<int,GameObject> playerDict = new Dictionary<int,GameObject>();

	void Start () {
		numPlayers = 0;
		blueTeam = GameObject.Find ("BlueTeamBase").GetComponent<Team>();
		redTeam = GameObject.Find ("RedTeamBase").GetComponent<Team>();
	}

	public GameObject GetHero(int playerID) {
		GameObject hero;
		playerDict.TryGetValue(playerID, out hero);
		return hero;
	}

	#region IPlayerJoin implementation
	public void PlayerJoin (int playerID) {
		GameObject hero;
		if (blueTeam.numPlayersOnTeam < redTeam.numPlayersOnTeam) {
			hero = blueTeam.CreateHero (playerID);
		} else {
			hero = redTeam.CreateHero (playerID);
		}

		playerDict.Add (numPlayers, hero);
		numPlayers++;
	}
	#endregion
}
