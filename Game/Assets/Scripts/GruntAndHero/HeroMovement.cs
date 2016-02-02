
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public enum MoveDirection {
	E, SE, S, SW, W, NW, N, NE, NONE
}

public class HeroMovement : NetworkBehaviour, IHeroMovement
{
    private Vector3 movementTarget;

	private Stats stats;
    
    private LayerMask terrainMask = -1;
    
    private MoveDirection moveDirection = MoveDirection.NONE;
    
    private float moveUnit = 0.1f;

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
                updatePosition();
                break;
            case GameState.State.END:
                if (isServer) gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                break;
        }
	}
    
    private void updatePosition(){
         Vector3 newPosition = transform.position;
         switch (moveDirection){
             case MoveDirection.E:
                newPosition.x += moveUnit;
                break;
             case MoveDirection.SE:
                newPosition.x += Mathf.Sqrt(moveUnit);
                newPosition.z -= Mathf.Sqrt(moveUnit);
                break;
             case MoveDirection.S:
                newPosition.z -= moveUnit;
                break;
             case MoveDirection.SW:
                newPosition.x -= Mathf.Sqrt(moveUnit);
                newPosition.z -= Mathf.Sqrt(moveUnit);
                break;
             case MoveDirection.W:
                newPosition.x -= moveUnit;
                break;
             case MoveDirection.NW:
                newPosition.x -= Mathf.Sqrt(moveUnit);
                newPosition.z += Mathf.Sqrt(moveUnit);
                break;
             case MoveDirection.N:
                newPosition.z += moveUnit;
                break;
             case MoveDirection.NE:
                newPosition.x += Mathf.Sqrt(moveUnit);
                newPosition.z += Mathf.Sqrt(moveUnit);
                break;
         }
         transform.position = newPosition; //AdjustToTerrain(newPosition);
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
	
	private Vector3 AdjustToTerrain (Vector3 movementTargetInput) {
        RaycastHit terrainLevel;
        movementTargetInput.y = 20f;
        if(Physics.Raycast(movementTargetInput, -Vector3.up, out terrainLevel, 21f, terrainMask)) movementTargetInput = terrainLevel.point;
        return movementTargetInput;
	}

    // implement movement interface
    public void PlayerMovement(MoveDirection moveDirection)
    {
        this.moveDirection = moveDirection;
    }

}