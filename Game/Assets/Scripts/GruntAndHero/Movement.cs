using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Movement : NetworkBehaviour{

	private NavMeshAgent agent;

	[SyncVar] private Vector3 movementTarget;
	[SyncVar] public bool isInitialised = false;
	[SyncVar] private Vector3 synchPos;
	[SyncVar] private float synchYRot;
	
	private Vector3 lastPos;
	private Quaternion lastRot;
	public float lerpRate = 10f;
	public float positionThreshold = 0.5f;
	public float rotationThreshold = 5f;
	
	private Stats stats;

	void Start() {
        if (isServer) {
            gameObject.GetComponent<Rigidbody>().useGravity = true;
        }
		stats = (Stats) GetComponent<Stats>();
		synchPos = transform.position;
	}

	void Update(){
		if (isServer) {
			SeverSetNewPosition();
		} else {
			if(NotTooClose()){
				ClientMoveToPosition();
			}
		}
	}

	private void SeverSetNewPosition(){
		transform.position = Vector3.Lerp (transform.position, this.movementTarget, Time.deltaTime);
		if (Vector3.Distance (transform.position, lastPos) > positionThreshold
			|| Quaternion.Angle (transform.rotation, lastRot) > rotationThreshold) {
			lastPos = transform.position;
			lastRot = transform.rotation;
		
			synchPos = transform.position;
			synchYRot = transform.localEulerAngles.y;
		}
		/*float step = speed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, movementTarget, step);*/
	}

	private void ClientMoveToPosition(){
		transform.position = Vector3.Lerp (transform.position, synchPos, Time.deltaTime * lerpRate);
		transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (new Vector3 (0, synchYRot, 0)), Time.deltaTime * lerpRate);
	}

	private bool NotTooClose(){
		if (Vector3.Distance (transform.position, movementTarget) > stats.minDistanceFromEnemy) {
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

