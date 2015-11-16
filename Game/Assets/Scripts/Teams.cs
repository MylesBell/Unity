﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public enum TeamID {
	red, blue
}

public enum Direction {
	up, down
}

public class Teams : NetworkBehaviour, IPlayerJoin {

	Team blueTeam, redTeam;
	int numPlayers, numPlayersRed, numPlayersBlue;
	Dictionary<int,GameObject> playerDict = new Dictionary<int,GameObject>();
	UnitFactory unitFactory;

	void Start () {
		numPlayers = 0;
		numPlayersBlue = 0;
		numPlayersRed = 0;
		unitFactory = GetComponent<UnitFactory> ();
	}

	public GameObject GetHero(int playerID) {
		GameObject hero;
		playerDict.TryGetValue(playerID, out hero);
		return hero;
	}

	#region IPlayerJoin implementation
	public void PlayerJoin (int playerID) {
		GameObject hero;
		if (numPlayersBlue < numPlayersRed) {
			hero = unitFactory.CreateHero (TeamID.blue, playerID);
			numPlayersBlue++;
		} else {
			hero = unitFactory.CreateHero (TeamID.red, playerID);
			numPlayersRed++;
		}

		playerDict.Add (numPlayers, hero);
		numPlayers++;
	}
	#endregion
}
