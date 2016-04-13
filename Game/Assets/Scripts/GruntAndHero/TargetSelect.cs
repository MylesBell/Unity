using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TargetSelect : NetworkBehaviour {

	public TeamID teamID;
	private Attack attack;
	private Vector3 desiredPosition;

    public string attackGruntTag;
    public string attackHeroTag;
    public string attackBaseTag;
    private Stats stats;
    
    public float GruntMovementForwardMovePerUpdate = 1f;
    
    private Queue<Vector3> moveTargets = new Queue<Vector3>();
    public bool usePathFinding;
    public bool showPathFinding;
    private bool wasAttacking;
    private bool movementReset;
    
    private Vector3 prevPosition;
    private float notMovedSeconds;
    private float maxNotMovedSecondsBeforePanic = 2;
    private int vectorCount = 0;

	
	void Start() {
        if (isServer) {
            usePathFinding = GraniteNetworkManager.usePathFinding;
            showPathFinding = GraniteNetworkManager.showPathFinding;
            gameObject.GetComponent<Rigidbody>().useGravity = true;
        } else {
            gameObject.GetComponent<Rigidbody>().detectCollisions = false;
        }
		stats = (Stats) GetComponent<Stats>();
        wasAttacking = false;
	}
	
	public void InitialiseTargetSelect (TeamID teamIDInput, Vector3 desiredPosition)	{
		teamID = teamIDInput;
        this.desiredPosition = desiredPosition;
		attack = GetComponent<Attack> ();
        attackGruntTag = teamID == TeamID.blue ? "redGrunt" : "blueGrunt";
        attackHeroTag = teamID == TeamID.blue ? "redHero" : "blueHero";
        attackBaseTag = teamID == TeamID.blue ? "redBase" : "blueBase";
        if(usePathFinding && GetComponent<Grunt>()){
            GetComponent<GruntClientPathFinder>().StartPaths();
            moveTargets.Clear();
            wasAttacking = false;
            prevPosition = transform.position;
            notMovedSeconds = 0f;
            vectorCount = 0;
        }
    }

	void Update () {
        if (isServer && GameState.gameState == GameState.State.PLAYING) {
            attack.setTarget(GetNewAttackTarget());
            
            //automatic movement for grunts
            if(gameObject.GetComponent<Grunt>()){
                //do movement
                if (hasAttackTarget()) {
                    gameObject.GetComponent<GruntMovement>().SetTarget(attack.getTarget().GetComponent<Collider>().ClosestPointOnBounds(transform.position));
                    desiredPosition.x = transform.position.x;
                    wasAttacking = true;
                    movementReset = false;
                } else {
                    if(usePathFinding){
                        if (wasAttacking) MoveBackToTarget();
                        else UpdateMoveTargetPathFinding();
                    } else {
                        UpdateMoveTargetNoPathFinding();
                    }
                    prevPosition = transform.position;
                }
            }
        }
	}
    
    private void MoveBackToTarget(){
        if(!movementReset) ResetMoveTarget();
        gameObject.GetComponent<GruntMovement>().SetTarget(desiredPosition);
        if(Vector3.Distance(desiredPosition, transform.position) < 3.0f){
            GetComponent<GruntClientPathFinder>().StartPaths();
            wasAttacking = false;
        }
        
    }
    private void ResetMoveTarget(){
        GetComponent<GruntClientPathFinder>().StopPaths();
        GetComponent<GruntClientPathFinder>().ForceRequest(desiredPosition);
        moveTargets.Clear();
        movementReset = true;
    }
	
	private void UpdateMoveTargetPathFinding(){
		float distance = Vector3.Distance (desiredPosition, transform.position);
        if(distance < 2.0f && moveTargets.Count > 0) {
            desiredPosition = moveTargets.Dequeue();
            gameObject.GetComponent<GruntMovement>().SetTarget(desiredPosition);
            notMovedSeconds = 0;
        } else if(distance < 5.0f && moveTargets.Count == 0){
            if(Vector3.Distance (prevPosition, transform.position) < 1f) {
                if(notMovedSeconds == 0.0) UpdateMoveTargetNoPathFinding(); //force one move
                notMovedSeconds += Time.deltaTime;
                if(notMovedSeconds > maxNotMovedSecondsBeforePanic){
                    GetComponent<GruntClientPathFinder>().Panic();
                    notMovedSeconds = 0;
                }
            } else {
                notMovedSeconds = 0;
            }
        }
    }
    
    private void UpdateMoveTargetNoPathFinding(){
		float distance = Vector3.Distance (desiredPosition, transform.position);
        if (distance < 2.0f) {
            if (teamID == TeamID.blue){
                desiredPosition.x += GruntMovementForwardMovePerUpdate;
            } else {
                desiredPosition.x -= GruntMovementForwardMovePerUpdate;
            }
        }
        gameObject.GetComponent<GruntMovement>().SetTarget(desiredPosition);
    }

	private bool hasAttackTarget(){
		return (attack.getTarget () != null);
	}

	private GameObject GetNewAttackTarget(){
        Collider closestBase = null;
        Collider closestHero = null;
        Collider closestGrunt = null;
        float currentDistanceBase = Mathf.Infinity;
        float currentDistanceHero = Mathf.Infinity;
        float currentDistanceGrunt = Mathf.Infinity;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stats.targetSelectRange);
        foreach(Collider collider in hitColliders) {
            if (collider.gameObject.activeSelf && isAttackable(collider.gameObject)) { //check if active and attackable (used for invisibility)
                if (string.Equals(collider.gameObject.tag, attackGruntTag)) {
                    closestGrunt = closestCollider(closestGrunt, collider, ref currentDistanceGrunt);
                } else if (string.Equals(collider.gameObject.tag, attackHeroTag)) {
                    closestHero = closestCollider(closestHero, collider, ref currentDistanceHero);
                } else if (string.Equals(collider.gameObject.tag, attackBaseTag)) {
                    closestBase = closestCollider(closestBase, collider, ref currentDistanceBase);
                }
            }
        }
        // targets entities in priority order
        if (closestBase) return closestBase.gameObject;
        if (closestHero) return closestHero.gameObject;
        if (closestGrunt) return closestGrunt.gameObject;
        return null;
    }

    private Collider closestCollider(Collider currentCollider, Collider newCollider, ref float currentDistance) {
        float newDistance = distanceToCollider(newCollider);
        if(currentCollider == null || newDistance < currentDistance) {
            currentDistance = newDistance;
            return newCollider;
        }
        return currentCollider;
    }

    private float distanceToCollider(Collider collider) {
        return Vector3.Distance(collider.ClosestPointOnBounds(transform.position), transform.position);
    }
    
    public void AddToQueue(Vector3[] vectors){
        if(showPathFinding) RpcDrawPath(vectors);
        foreach(Vector3 vector in vectors){
            moveTargets.Enqueue(vector);
        }
    }
    
    [ClientRpc]
    private void RpcDrawPath(Vector3[] vectors) {
		if (vectors != null) {
            LineRenderer line;
            if(!(line = gameObject.GetComponent<LineRenderer>())){
                line = gameObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
            }
            line.material = new Material( Shader.Find( "Unlit/Color" ) ) { color = Color.yellow };
            line.SetWidth( 0.5f, 0.5f );
            line.SetColors( Color.yellow, Color.yellow );
            line.SetVertexCount( vectorCount + vectors.Length + 1 );
            line.SetPosition(vectorCount, transform.position);
			for (int i = 0; i < vectors.Length; i ++) {
                line.SetPosition(vectorCount+i+1, vectors[i]);
			}
		}
	}
    private bool isAttackable(GameObject enemy){
        if((enemy.tag == attackHeroTag) && gameObject.GetComponent<Grunt>()){
            return gameObject.GetComponent<Grunt>().team.isAttackable(enemy);
        }
        
        return true;
    }
}
