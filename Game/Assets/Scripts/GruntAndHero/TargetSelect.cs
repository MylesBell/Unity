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
	
	void Start() {
        if (isServer) {
            gameObject.GetComponent<Rigidbody>().useGravity = true;
        } else {
            gameObject.GetComponent<Rigidbody>().detectCollisions = false;
        }
		stats = (Stats) GetComponent<Stats>();
	}
	
	public void InitialiseTargetSelect (TeamID teamIDInput, Vector3 desiredPosition)	{
		teamID = teamIDInput;
        this.desiredPosition = desiredPosition;
		attack = GetComponent<Attack> ();
        attackGruntTag = teamID == TeamID.blue ? "redGrunt" : "blueGrunt";
        attackHeroTag = teamID == TeamID.blue ? "redHero" : "blueHero";
        attackBaseTag = teamID == TeamID.blue ? "redBase" : "blueBase";
    }

	void Update () {
        if (isServer) {
            attack.setTarget(GetNewAttackTarget());
            
            //automatic movement for grunts
            if(gameObject.GetComponent<Grunt>()){
                //do movement
                if (hasAttackTarget()) {
                    gameObject.GetComponent<GruntMovement>().SetTarget(attack.getTarget().GetComponent<Collider>().ClosestPointOnBounds(transform.position));
                    desiredPosition.x = transform.position.x;
                } else {
                    UpdateMoveTarget();
                }
            }
        }
	}
	
	private void UpdateMoveTarget(){
        
		float distance = Vector3.Distance (desiredPosition, transform.position);
		
		if (distance < 2.0f) {
			if (teamID == TeamID.blue){
                desiredPosition.x += GruntMovementForwardMovePerUpdate;
			}else{
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
            if (collider.gameObject.activeSelf) { //check if active
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
}
