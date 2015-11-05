using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour {
	public float speed = 1.0f;
	public float health = 5.0f;
	public float maxHealth = 5.0f;
	public TeamID teamID;
	private NavMeshAgent agent;
	private Vector3 tempTargetLocation;
	
	void Update () {
		//Temporary until NavMesh generation is implemented in terrain data
		transform.position = Vector3.Lerp (transform.position, tempTargetLocation, Time.deltaTime * speed);
	}
	
	public void InitialiseGrunt(TeamID id, Vector3 targetPosition) {
		SetTeam (id);
		SetTargetPosition (targetPosition);
	}
	
	void SetTeam(TeamID id) {
		teamID = id;
		Material mat = GetComponent<Renderer> ().material;
		if (id == TeamID.red) {
			mat.SetColor ("_Color", Color.red);
		} else {
			mat.SetColor ("_Color", Color.blue);
		}
	}

	void SetTargetPosition (Vector3 targetPosition) {
		//Temporary until NavMesh generation is implemented in terrain data
		tempTargetLocation = targetPosition;
		NavMeshAgent agent = GetComponent<NavMeshAgent> ();
//		agent.destination = targetPosition;
	}
}