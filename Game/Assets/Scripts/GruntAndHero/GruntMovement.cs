using UnityEngine;
using UnityEngine.Networking;

public class GruntMovement : NetworkBehaviour{
	private Vector3 movementTarget;

	private Stats stats;
    
    private LayerMask terrainMask = 256;

	void Start() {
		stats = (Stats) GetComponent<Stats>();
	}

    public void initialiseMovement(Vector3 position) {
        transform.position = position;
        movementTarget = position;
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        CmdSetPositionOnClient();
    }

    [Command]
    public void CmdSetPositionOnClient() {
        RpcRecievePosition(transform.position);
    }

    [ClientRpc]
    public void RpcRecievePosition(Vector3 position) {
        transform.position = position;
    }

    void Update(){
        switch (GameState.gameState) {
            case GameState.State.IDLE:
                if (isServer) gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                break;
            case GameState.State.PLAYING:
                break;
            case GameState.State.END:
                if (isServer) gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                break;
        }
        if (isServer && NotTooClose()){
            if(GameState.gameState == GameState.State.PLAYING) {
                transform.position = Vector3.Lerp (transform.position, movementTarget, Time.deltaTime * stats.movementSpeed / 5.0f);
            }
        }
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
        RaycastHit terrainLevel;
        movementTargetInput.y = 20f;
        if(Physics.Raycast(movementTargetInput, -Vector3.up, out terrainLevel, 21f, terrainMask)) movementTargetInput = terrainLevel.point;
        movementTargetInput.y += GetComponent<Renderer>().bounds.size.y/2;
        movementTarget = movementTargetInput;
	}
}

