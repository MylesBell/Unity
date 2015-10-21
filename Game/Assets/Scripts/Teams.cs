using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TeamID {
	red, blue
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
		return playerDict [playerID];
	}

	#region IPlayerJoin implementation
	public void PlayerJoin () {
		GameObject hero;
		if (blueTeam.numPlayersOnTeam < redTeam.numPlayersOnTeam) {
			hero = blueTeam.CreateHero ();
		} else {
			hero = redTeam.CreateHero ();
		}

		playerDict.Add (numPlayers, hero);
		numPlayers++;
	}
	#endregion
}
