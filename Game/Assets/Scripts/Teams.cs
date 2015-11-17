using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public enum TeamID {
	red, blue
}

public enum Channel {
	up, down
}

public class Teams : NetworkBehaviour, IPlayerJoin {

	Team blueTeam, redTeam;
	int numPlayers, numPlayersRed, numPlayersBlue;
	Dictionary<string,GameObject> playerDict = new Dictionary<string,GameObject>();
	UnitFactory unitFactory;

	void Start () {
		numPlayers = 0;
		numPlayersBlue = 0;
		numPlayersRed = 0;
		unitFactory = GetComponent<UnitFactory> ();
	}

	public GameObject GetHero(string playerID) {
		GameObject hero;
		playerDict.TryGetValue(playerID, out hero);
		return hero;
	}

	#region IPlayerJoin implementation
	public void PlayerJoin (string playerID) {
		GameObject hero;
		if (numPlayersBlue < numPlayersRed) {
			hero = unitFactory.CreateHero (TeamID.blue);
			numPlayersBlue++;
		} else {
			hero = unitFactory.CreateHero (TeamID.red);
			numPlayersRed++;
		}

		playerDict.Add (playerID, hero);
		numPlayers++;
	}
	#endregion
}
