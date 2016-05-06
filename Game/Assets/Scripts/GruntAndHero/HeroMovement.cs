using UnityEngine;
using UnityEngine.Networking;

public enum MoveDirection {
	E, SE, S, SW, W, NW, N, NE, NONE
}

public class HeroMovement : NetworkBehaviour, IHeroMovement
{
    private TargetSelect targetSelect;
	private Stats stats;
    public Animator animator;
    
    private LayerMask terrainMask = 256;
    
    public MoveDirection moveDirection;
    public Vector3 currentMovement;
    private Vector3 lastPosition;
    
    public float testSpeed;
    public float moveUnit = 1.0f;
    
    private ComputerLane computerLane;

    void Start () {
        targetSelect = GetComponent<TargetSelect>();
        stats = GetComponent<Stats>();
    }
    
    public void SwitchAnimator(Animator anim) {
        animator = anim;
    }

    public void initialiseMovement(Vector3 position) {
        transform.position = position;
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        if(isServer) moveDirection = MoveDirection.NONE;
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
    
    [ClientRpc]
    public void RpcResetAnimator() {
        animator.SetBool("Victory", false);                    
        animator.SetBool("Defeat", false); 
    }
   
    
    public void SetAnimatorSpeed(float speed) {
        animator.SetFloat("Speed", speed);  
    }
    
    [ClientRpc]
    public void RpcSetEndAnim(TeamID teamID) {
        if (teamID == GameState.winningTeam) {
            animator.SetBool("Victory", true);
        } else {
            animator.SetBool("Defeat", true);
        }
    }

    void FixedUpdate(){
        if(isServer) {
            switch (GameState.gameState) {
                case GameState.State.IDLE:
                    gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero; 
                    RpcResetAnimator();
                    break;
                case GameState.State.PLAYING:
                    updatePosition();
                    break;
                case GameState.State.END:
                    gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    currentMovement = Vector3.zero;
                    RpcSetEndAnim(targetSelect.teamID);
                    break;
            }
        }
	}
    
    void LateUpdate() {
        SetAnimatorSpeed((transform.position - lastPosition).magnitude/Time.deltaTime);
        lastPosition = transform.position; 
    }
    
    private void updatePosition(){      
         if (moveDirection != MoveDirection.NONE){
             currentMovement = getMovementUpdate();
         }else{
             currentMovement = getIdleMovementUpdate();
         }
         if (currentMovement != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(currentMovement);
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
	
	public Vector3 AdjustToTerrain (Vector3 movementTargetInput) {
        RaycastHit terrainLevel;
        movementTargetInput.y = 20f;
        if(Physics.Raycast(movementTargetInput, -Vector3.up, out terrainLevel, 30f, terrainMask)){
            movementTargetInput = terrainLevel.point;
        }
        movementTargetInput.y += GetComponentInChildren<Renderer>().bounds.size.y/2 - (GetComponentInChildren<Renderer>().bounds.center.y);
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