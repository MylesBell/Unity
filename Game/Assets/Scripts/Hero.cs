using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour, IHeroMovement {
	// characteristics
	public float speed = 10.0f;
	public TeamID teamID;
	public int minDistance;
	public string playerType = "elf";

	//movement
	private NavMeshAgent agent;
	private GameObject target;
	private Vector3 targetLocation;
	
	// attack
	private Attack heroAttack;

	void Start () {
		target = null;
		heroAttack = this.GetComponent<Attack> ();
	}
	
	void Update () {
		if (target == null) {
			getNewTarget ();
		} else {
			SetTargetPosition(target.transform.position);
		}
		moveTowardsTarget ();
		//Temporary until NavMesh generation is implemented in terrain data
		//transform.position = Vector3.Lerp (transform.position, tempTargetLocation, Time.deltaTime * speed);
	}

	public void InitialiseHero(TeamID teamID, Vector3 targetPosition) {
		SetTeam (teamID);
		SetTargetPosition (targetPosition);
	}

	private void SetTeam(TeamID id) {
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

	private void SetTargetPosition (Vector3 targetLocationInput) {
		//Temporary until NavMesh generation is implemented in terrain data
		targetLocation = targetLocationInput;
		//NavMeshAgent agent = GetComponent<NavMeshAgent> ();
		//agent.destination = targetPosition;
	}

	private void moveTowardsTarget(){
		//print ("move " + transform.position + " to " + targetLocation);
		if(Vector3.Distance(transform.position, targetLocation) > minDistance){
			float step = speed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, targetLocation, step);
		}
	}
	
	private void getNewTarget(){
		target = getClosestEnemyGrunt ();
		if (target == null) {
			target = getClosestEnemyHero();
		}
		if (target == null) {
			target = getEnemyBase();
		}
		if (target != null) {
			heroAttack.setTarget (target);
		}
	}
	
	private GameObject getClosestEnemyGrunt(){
		if (teamID == TeamID.blue) {
			target = FindClosestObjectWithTag("redGrunt");
		} else {
			target = FindClosestObjectWithTag("blueGrunt");
		}
		return target;
	}
	
	private GameObject getClosestEnemyHero(){
		if (teamID == TeamID.blue) {
			target = FindClosestObjectWithTag("redHero");
		} else {
			target = FindClosestObjectWithTag("blueHero");
		}
		return target;
	}
	
	private GameObject getEnemyBase(){
		if (teamID == TeamID.blue) {
			target = FindClosestObjectWithTag("redBase");
		} else {
			target = FindClosestObjectWithTag("blueBase");
		}
		return target;
	}
	
	private GameObject FindClosestObjectWithTag(string type) {
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag(type);
		GameObject closest = null;
		float distance = Mathf.Infinity;
		Vector3 position = transform.position;
		foreach (GameObject go in gos) {
			Vector3 diff = go.transform.position - position;
			float curDistance = diff.sqrMagnitude;
			if (curDistance < distance) {
				closest = go;
				distance = curDistance;
			}
		}
		return closest;
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
