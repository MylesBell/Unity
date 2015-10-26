using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Team : NetworkBehaviour {
	
//	public TeamID teamID;
//	public GameObject heroPrefab, gruntPrefab;
//	public int numPlayersOnTeam;
//
//	// Use this for initialization
//	void Start () {
//		numPlayersOnTeam = 0;
//	}
//	
//	public GameObject CreateHero(int newPlayerID) {
//		GameObject heroObject = Instantiate (heroPrefab, transform.position + new Vector3(4,0,0), Quaternion.identity) as GameObject;
//		NetworkServer.Spawn (heroObject);
//		Hero hero = heroObject.GetComponent<Hero> ();
//		hero.InitialiseHero (teamID);
//		numPlayersOnTeam++;
//		return heroObject;
//	}
//
//	public GameObject CreateGrunt() {
//		GameObject gruntObject = Instantiate (gruntPrefab,transform.position + new Vector3(4,0,0), Quaternion.identity) as GameObject;
//		NetworkServer.Spawn (gruntObject);
//		Grunt grunt = gruntObject.GetComponent<Grunt> ();
//		grunt.InitialiseGrunt (teamID);
//		return gruntObject;
//	}
}
