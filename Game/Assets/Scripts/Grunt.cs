using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour {
	// characteristics
	public float speed = 20.0f;
	public TeamID teamID;
	public int minDistance;

	// movement
	private NavMeshAgent agent;
	private GameObject target;
	private Vector3 targetLocation;

	// attack
	private GruntAttack gruntAttack;
	
	void Start () {
		target = null;
		minDistance = 5;
		gruntAttack = this.GetComponent<GruntAttack> ();
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

	private void SetTargetPosition (Vector3 targetLocationIn) {
		//Temporary until NavMesh generation is implemented in terrain data
		targetLocation = targetLocationIn;
		//NavMeshAgent agent = GetComponent<NavMeshAgent> ();
		//agent.destination = targetPosition;
	}

	void moveTowardsTarget(){
		//print ("move " + transform.position + " to " + targetLocation);
		if(Vector3.Distance(transform.position, targetLocation) > minDistance){
			float step = speed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, targetLocation, step);
		}
	}

	void getNewTarget(){
		if (teamID == TeamID.blue) {
			target = FindClosestObjectWithTag("blueGrunt");
		} else {
			target = FindClosestObjectWithTag("redGrunt");
		}
		if (target == null) {
			if (teamID == TeamID.blue) {
				target = GameObject.FindGameObjectWithTag("redBase");
			} else {
				target = GameObject.FindGameObjectWithTag("blueBase");
			}
		}
		if (target != null) {
			gruntAttack.setTarget (target);
		}
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
}