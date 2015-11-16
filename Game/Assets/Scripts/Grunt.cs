using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour {
	[SyncVar] public float speed = 1.0f;
	[SyncVar] public float health = 5.0f;
	[SyncVar] public TeamID teamID;
	private NavMeshAgent agent;
	[SyncVar] public Vector3 tempTargetLocation;
	[SyncVar] public bool isInitialised = false;

	[SyncVar] private Vector3 synchPos;
	[SyncVar] private float synchYRot;
	
	private Vector3 lastPos;
	private Quaternion lastRot;
	public float lerpRate = 10f;
	public float positionThreshold = 0.5f;
	public float rotationThreshold = 5f;

	void Start() {
		synchPos = transform.position;
	}

	void Update() {
		if (isInitialised) {
			if (isServer) {
				transform.position = Vector3.Lerp (transform.position, this.tempTargetLocation, Time.deltaTime);
				if (Vector3.Distance (transform.position, lastPos) > positionThreshold
					|| Quaternion.Angle (transform.rotation, lastRot) > rotationThreshold) {
					lastPos = transform.position;
					lastRot = transform.rotation;
				
					synchPos = transform.position;
					synchYRot = transform.localEulerAngles.y;
				}
			} else {
				transform.position = Vector3.Lerp (transform.position, synchPos, Time.deltaTime * lerpRate);
				transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (new Vector3 (0, synchYRot, 0)), Time.deltaTime * lerpRate);
			}
		}
	}
	
	public void InitialiseGrunt(TeamID id, Vector3 targetPosition) {
		SetTeam (id);
		SetTargetPosition (targetPosition);
		isInitialised = true;
	}
	
	void SetTeam(TeamID id) {
		this.teamID = id;
//		mat = this.GetComponent<Renderer> ().material;
//		if (id == TeamID.red) {
//			mat.SetColor ("_Color", Color.red);
//		} else {
//			mat.SetColor ("_Color", Color.blue);
//		}
	}

	void SetTargetPosition (Vector3 targetPosition) {
		//Temporary until NavMesh generation is implemented in terrain data
		this.tempTargetLocation = targetPosition;
		NavMeshAgent agent = GetComponent<NavMeshAgent> ();
//		agent.destination = targetPosition;
	}

	void SetTargetPosition (Vector3 targetPosition) {
		//Temporary until NavMesh generation is implemented in terrain data
		tempTargetLocation = targetPosition;
		NavMeshAgent agent = GetComponent<NavMeshAgent> ();
//		agent.destination = targetPosition;
	}
}