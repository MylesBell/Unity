using UnityEngine;
using UnityEngine.Networking;

public class GruntMovement : NetworkBehaviour{
    
	private Vector3 movementTarget;
    private Vector3 currentMovement;
    private Vector3 lastPosition;

    private TargetSelect targetSelect;
	private Stats stats;
    private Animator animator;
    
    // private LayerMask terrainMask = 256;

	void Start() {
        targetSelect = GetComponent<TargetSelect>();
		stats = GetComponent<Stats>();
        animator = GetComponentInChildren<Animator>();
        animator.enabled = true;
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
    
    [ClientRpc]
    public void RpcResetAnimator() {
        // animator = GetComponentInChildren<Animator>();
        animator.SetBool("Victory", false);                    
        animator.SetBool("Defeat", false); 
    }
    
    [ClientRpc]
    public void RpcSetAnimatorSpeed(float speed) {
        Component[] componentsOfChildren = transform.GetComponentsInChildren<Component>();
        Debug.Log(componentsOfChildren.Length);
        // animator = GetComponentInChildren<Animator>();
        animator.SetFloat("Speed", speed);         
    }
    public void SetAnimatorSpeed(float speed) {
        animator.SetFloat("Speed", speed);         
    }
    [ClientRpc]
    public void RpcSetEndAnim(TeamID teamID) {
        // animator = GetComponentInChildren<Animator>();
        if (teamID == GameState.winningTeam)
            animator.SetBool("Victory", true);
        else
            animator.SetBool("Defeat", true);
    }

    void LateUpdate(){
        if(isServer) {
            switch (GameState.gameState) {
                case GameState.State.IDLE:
                    gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    RpcResetAnimator();  
                    break;
                case GameState.State.PLAYING:                            
                    break;
                case GameState.State.END:
                    gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    currentMovement = Vector3.zero;
                    RpcSetEndAnim(targetSelect.teamID);               
                    break;
            }
            // RpcSetAnimatorSpeed(currentMovement.magnitude);
            if (NotTooClose()){
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
        SetAnimatorSpeed((transform.position - lastPosition).magnitude/Time.deltaTime);
        lastPosition = transform.position;
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