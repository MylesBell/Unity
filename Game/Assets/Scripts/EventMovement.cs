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
		float moveX = Input.GetAxis ("Horizontal") * speed;
		float moveY = Input.GetAxis ("Vertical") * speed;
		if (teams.GetHero(playerID) != null)
			ExecuteEvents.Execute<IHeroMovement> (teams.GetHero(playerID), null, (x,y) => x.Move (moveX, moveY));
	}
}
