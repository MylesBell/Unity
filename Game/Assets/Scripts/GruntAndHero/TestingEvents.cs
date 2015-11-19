using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class EventMovement : MonoBehaviour {

	public float speed;
	public Teams teams;
	public string playerID = "test";

	// Use this for initialization
	void Start () {
		teams = FindObjectOfType<Teams> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (teams.GetHero (playerID) != null){
			MoveDirection moveDirection;
			if (Input.GetKeyUp (KeyCode.UpArrow)) {
                moveDirection = MoveDirection.up;
				ExecuteEvents.Execute<IHeroMovement> (teams.GetHero (playerID), null, (x,y) => x.PlayerMoveChannel (moveDirection));
			} else if (Input.GetKeyUp (KeyCode.DownArrow)) {
                moveDirection = MoveDirection.down;
				ExecuteEvents.Execute<IHeroMovement> (teams.GetHero (playerID), null, (x,y) => x.PlayerMoveChannel (moveDirection));
			}
		}
	}
}
