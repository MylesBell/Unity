using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Movement : NetworkBehaviour{

	[SyncVar] private Vector3 movementTarget;
	private NavMeshAgent agent;

	// characteristics, move to stats later
	public float speed = 5.0f;
	public int minDistance;

	[SyncVar] public bool isInitialised = false;

	[SyncVar] private Vector3 synchPos;
	[SyncVar] private float synchYRot;
	
	private Vector3 lastPos;
	private Quaternion lastRot;
	public float lerpRate = 10f;
	public float positionThreshold = 0.5f;
	public float rotationThreshold = 5f;

	void Start() {
        if (isServer) {
            gameObject.GetComponent<Rigidbody>().useGravity = true;
        }
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
		Debug.Log(movementTargetInput);
		movementTarget = movementTargetInput;
	}
}

