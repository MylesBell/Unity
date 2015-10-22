using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CreateHero : MonoBehaviour {

	public GameObject teamsObject;
	int playerCounter;

	// Use this for initialization
	void Start () {
		playerCounter = 0;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.P)) {
			ExecuteEvents.Execute<IPlayerJoin> (teamsObject, null, (x,y) => x.PlayerJoin(playerCounter));
			playerCounter++;
		}
	}
}
