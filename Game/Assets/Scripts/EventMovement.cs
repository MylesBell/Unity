using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class EventMovement : MonoBehaviour {

	public float speed;
	public Teams teams;
	public int playerID = 0;

	// Use this for initialization
	void Start () {
		teams = FindObjectOfType<Teams> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (teams.GetHero (playerID) != null){
			Direction direction;
			if (Input.GetKeyUp (KeyCode.UpArrow)) {
				direction = Direction.up;
				ExecuteEvents.Execute<IHeroMovement> (teams.GetHero (playerID), null, (x,y) => x.PlayerMoveLane (direction));
			} else if (Input.GetKeyUp (KeyCode.DownArrow)) {
				direction = Direction.down;
				ExecuteEvents.Execute<IHeroMovement> (teams.GetHero (playerID), null, (x,y) => x.PlayerMoveLane (direction));
			}
		}
	}
}
