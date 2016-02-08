
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
    
    private LayerMask terrainMask = 256;
    
    public MoveDirection moveDirection;
    public Vector3 currentMovement;
    public float moveUnit = 1.0f;
    
    private ComputerLane computerLane;

    void Start ()
    {
        stats = (Stats) GetComponent<Stats>();
    }

    public void initialiseMovement(Vector3 position) {
        transform.position = position;
        movementTarget = position;
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        moveDirection = MoveDirection.NONE;
        CmdSetPositionOnClient();
    }
    
    public void setComputerLane(ComputerLane computerLane){
        this.computerLane = computerLane;
    }

    [Command]
    public void CmdSetPositionOnClient() {
        RpcRecievePosition(transform.position);
    }

    [ClientRpc]
    public void RpcRecievePosition(Vector3 position) {
        transform.position = position;
    }

    void FixedUpdate(){
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
        
         if (moveDirection != MoveDirection.NONE){
             currentMovement = getMovementUpdate();
         }else{
             currentMovement = getIdleMovementUpdate();
         }
         
         transform.position = AdjustToTerrain(currentMovement * Time.deltaTime + transform.position);     
    }
    
    private Vector3 getMovementUpdate(){
         Vector3 movement = Vector3.zero;
         switch (moveDirection){
             case MoveDirection.E:
                movement.x += moveUnit;
                break;
             case MoveDirection.SE:
                movement.x += moveUnit;
                movement.z -= moveUnit;
                break;
             case MoveDirection.S:
                movement.z -= moveUnit;
                break;
             case MoveDirection.SW:
                movement.x -= moveUnit;
                movement.z -= moveUnit;
                break;
             case MoveDirection.W:
                movement.x -= moveUnit;
                break;
             case MoveDirection.NW:
                movement.x -= moveUnit;
                movement.z += moveUnit;
                break;
             case MoveDirection.N:
                movement.z += moveUnit;
                break;
             case MoveDirection.NE:
                movement.x += moveUnit;
                movement.z += moveUnit;
                break;
         }
         movement = movement.normalized;
         
         currentMovement = Vector3.MoveTowards(currentMovement, movement * stats.movementSpeed, stats.movementAcceleration * Time.deltaTime);
         return currentMovement;
    }
    
    private Vector3 getIdleMovementUpdate(){
        // Apply friction to slow us to a halt
        currentMovement = Vector3.MoveTowards(currentMovement, Vector3.zero, 10.0f * Time.deltaTime);
        return currentMovement;
    }
	
	private Vector3 AdjustToTerrain (Vector3 movementTargetInput) {
        RaycastHit terrainLevel;
        movementTargetInput.y = 20f;
        if(Physics.Raycast(movementTargetInput, -Vector3.up, out terrainLevel, 21f, terrainMask)){
            movementTargetInput = terrainLevel.point;
        }
        movementTargetInput.y += GetComponent<Renderer>().bounds.size.y/2;
        return movementTargetInput;
	}

    // implement movement interface
    public void PlayerMovement(MoveDirection moveDirection)
    {
        //see if need to flip the direction
        if(moveDirection != MoveDirection.NONE && computerLane == ComputerLane.LEFT){
            int newDirection = ((int) moveDirection + 4 ) % 8;
            moveDirection = (MoveDirection) newDirection;
        }
        this.moveDirection = moveDirection;
    }

}