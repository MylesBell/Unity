using UnityEngine;
using UnityEngine.Networking;

public class GruntMovement : NetworkBehaviour{
	private Vector3 movementTarget;
    private Vector3 currentMovement;

	private Stats stats;
    
    // private LayerMask terrainMask = 256;

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
                currentMovement = Vector3.MoveTowards (currentMovement,
                    (movementTarget - transform.position).normalized * stats.movementSpeed, Time.deltaTime * stats.movementAcceleration);
                Vector3 newPosition = currentMovement * Time.deltaTime + transform.position;
                transform.LookAt(new Vector3(newPosition.x,transform.position.y,newPosition.z));
                newPosition = AdjustToTerrain(newPosition);
                transform.position = newPosition;
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
        movementTarget = movementTargetInput;
	}
    
    private Vector3 AdjustToTerrain(Vector3 movementTargetInput){
        // RaycastHit terrainLevel;
        // movementTargetInput.y = 20f;
        // if(Physics.Raycast(movementTargetInput, -Vector3.up, out terrainLevel, 21f, terrainMask)) movementTargetInput = terrainLevel.point;
        movementTargetInput.y = GetComponent<BoxCollider>().bounds.size.y/2;
        return movementTargetInput;
    }
}