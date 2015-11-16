using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour{

	private Vector3 movementTarget;
	private NavMeshAgent agent;

	// characteristics, move to stats later
	public float speed = 10.0f;
	public int minDistance;

	void Update(){
		moveTowardsTarget ();
	}

	private void moveTowardsTarget(){
		if(notTooClose()){
			float step = speed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, movementTarget, step);
		}
	}

	private bool notTooClose(){
		if (Vector3.Distance (transform.position, movementTarget) > minDistance) {
			return true;
		}
		return false;
	}

	// getters and setters
	public Vector3 GetTarget(){
		return movementTarget;
	}
	
	public void SetTarget (Vector3 movementTargetInput) {
		movementTarget = movementTargetInput;
	}
}

