using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour, IHeroMovement {
	public TeamID teamID;
	public float speed = 3.0f;
	public float health = 10.0f;
	public string playerType = "elf";
	private NavMeshAgent agent;
	private Vector3 tempTargetLocation;
	
	
	void Update () {
		//Temporary until NavMesh generation is implemented in terrain data
		transform.position = Vector3.Lerp (transform.position, tempTargetLocation, Time.deltaTime * speed);
	}

	public void InitialiseHero(TeamID teamID, Vector3 targetPosition) {
		SetTeam (teamID);
		SetTargetPosition (targetPosition);
	}

	void SetTeam(TeamID id) {
		teamID = id;
		Material mat = GetComponent<Renderer> ().material;
		Light light = transform.GetComponentInChildren<Light> ();
		if (id == TeamID.red) {
			mat.SetColor ("_Color", Color.red);
			mat.SetColor("_EmissionColor", Color.red);
			light.color = Color.red;
		} else {
			mat.SetColor ("_Color", Color.blue);
			mat.SetColor("_EmissionColor", Color.blue);
			light.color = Color.blue;
			
		}
	}

	void SetTargetPosition (Vector3 targetPosition) {
		//Temporary until NavMesh generation is implemented in terrain data
		tempTargetLocation = targetPosition;
		NavMeshAgent agent = GetComponent<NavMeshAgent> ();
		//		agent.destination = targetPosition;
	}
	
	#region IHeroMovement implementation
	public void PlayerBack ()
	{
		throw new System.NotImplementedException ();
	}
	public void PlayerMoveLane (Direction direction)
	{
		throw new System.NotImplementedException ();
	}
	#endregion
}
