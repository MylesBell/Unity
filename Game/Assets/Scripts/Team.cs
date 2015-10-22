using UnityEngine;
using System.Collections;

public class Team : MonoBehaviour {

	public TeamID teamID;
	public GameObject heroPrefab;
	public int numPlayersOnTeam;

	// Use this for initialization
	void Start () {
		numPlayersOnTeam = 0;
	}
	
	public GameObject CreateHero(int newPlayerID) {
		GameObject heroObject = Instantiate (heroPrefab, transform.GetChild(0).position, Quaternion.identity) as GameObject;
		Hero hero = heroObject.GetComponent<Hero> ();
		hero.InitialiseHero (teamID);
		numPlayersOnTeam++;
		return heroObject;
	}
}
