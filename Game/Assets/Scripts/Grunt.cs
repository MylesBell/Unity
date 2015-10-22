using UnityEngine;
using System.Collections;

public class Grunt : MonoBehaviour {

	public float speed = 1.0f;
	public float health = 5.0f;
	public TeamID teamID;
	private NavMeshAgent agent;
	

	
	void Start () {
	}
	
	public void InitialiseGrunt(TeamID id) {
		SetTeam (id);
	}
	
	void SetTeam(TeamID id) {
		teamID = id;
		Transform opposingBaseTransform;
		agent = GetComponent<NavMeshAgent> ();
		
		if (id == TeamID.blue) {
			opposingBaseTransform = GameObject.Find ("RedTeamBase").transform;
			agent.destination = opposingBaseTransform.GetChild (0).position;
		} else {
			opposingBaseTransform = GameObject.Find ("BlueTeamBase").transform;
			agent.destination = opposingBaseTransform.GetChild (0).position;
		}
		Material mat = GetComponent<Renderer> ().material;
		if (id == TeamID.red) {
			mat.SetColor ("_Color", Color.red);
		} else {
			mat.SetColor ("_Color", Color.blue);
		}
	}
}