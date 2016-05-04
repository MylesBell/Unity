using UnityEngine;
using UnityEngine.Networking;

public class SynchronisedMovement : NetworkBehaviour{
	[SyncVar] private Vector3 synchPos;
	[SyncVar] private Vector3 synchRot;
	
	private Vector3 lastPos;
	private Quaternion lastRot;
	public float lerpRate = 10f;
	public float positionThreshold = 0.5f;
	public float rotationThreshold = 5f;

    void Start(){
        synchPos = transform.position;
    }
    
    void Update(){
		if (isServer) SeverSetNewPosition();
		else ClientMoveToPosition();
	}
    
    public void ResetMovement(Vector3 position, Vector3 rotation){
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
		synchPos = position;
		synchRot = rotation;
		lastPos = position;
		lastRot = Quaternion.Euler(rotation);
    }

    [Command]
    public void CmdSetPosition(Vector3 position, Vector3 rotation) {
        RpcSetPosition(position, rotation);
    }

    [ClientRpc]
    public void RpcSetPosition(Vector3 position, Vector3 rotation) {
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
		synchPos = position;
		synchRot = rotation;
		lastPos = position;
		lastRot = Quaternion.Euler(rotation);
    }


	private void SeverSetNewPosition(){
		if (Vector3.Distance (transform.position, lastPos) > positionThreshold
			|| Quaternion.Angle (transform.rotation, lastRot) > rotationThreshold) {
			lastPos = transform.position;
			lastRot = transform.rotation;
		
			synchPos = transform.position;
			synchRot = transform.localEulerAngles;
		}
	}

	private void ClientMoveToPosition(){
		transform.position = Vector3.Lerp (transform.position, synchPos, Time.deltaTime * lerpRate);
		transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(synchRot), Time.deltaTime * lerpRate);
	}
}

