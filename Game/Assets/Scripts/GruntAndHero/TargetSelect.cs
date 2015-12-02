using UnityEngine;
using UnityEngine.Networking;

public class TargetSelect : NetworkBehaviour {

	private TeamID teamID;
	private Attack attack;
	private Movement movement;
	private float desiredZPosition;
	private Vector3 desiredPosition;
	private float zSeperation;
	private ProgressDirection progressDirection;

    private string attackGruntTag;
    private string attackHeroTag;
    private string attackBaseTag;
    private Stats stats;
	
	void Start() {
		stats = (Stats) GetComponent<Stats>();
	}
	
	public void InitialiseTargetSelect (TeamID teamIDInput, Vector3 desiredPosition, float zSeperation)
	{
		teamID = teamIDInput;
		this.desiredZPosition = desiredPosition.z;
        this.desiredPosition = desiredPosition;
		this.zSeperation = zSeperation;
		this.progressDirection = ProgressDirection.forward;
		attack = GetComponent<Attack> ();
		movement = GetComponent<Movement> ();
		movement.SetTarget (desiredPosition);
        attackGruntTag = teamID == TeamID.blue ? "redGrunt" : "blueGrunt";
        attackHeroTag = teamID == TeamID.blue ? "redHero" : "blueHero";
        attackBaseTag = teamID == TeamID.blue ? "redBase" : "blueBase";
    }
	
	public void SetProgressDirection(ProgressDirection progressDirection){
		this.progressDirection = progressDirection;
	}

	public void MoveToZOffset(MoveDirection moveDirection){
        switch (moveDirection) {
            case MoveDirection.up:
                if ((desiredZPosition + zSeperation) < Teams.maxZ) desiredZPosition += zSeperation;
                break;
            case MoveDirection.down:
                if ((desiredZPosition - zSeperation) > Teams.minZ) desiredZPosition -= zSeperation;
                break;
        }
        desiredPosition = new Vector3(transform.position.x, transform.position.y, desiredZPosition);
        movement.SetTarget(desiredPosition);
	}

	void Update () {
        if (isServer)
        {
            if (!hasAttackTarget()) {
                attack.setTarget(GetNewAttackTarget());
            }

            if (hasAttackTarget()) {
                movement.SetTarget(attack.getTarget().GetComponent<Collider>().ClosestPointOnBounds(transform.position));
            }
            else {
                UpdateMoveTarget();
            }
        }
	}
	
	private void UpdateMoveTarget(){
		float distance = Vector3.Distance (desiredPosition, transform.position);
		
		if (distance < 2.0f) {
			if (teamID == TeamID.blue){
				if (progressDirection == ProgressDirection.forward){
                	desiredPosition.x += zSeperation;
				}else{
					desiredPosition.x -= zSeperation;
				}
			}else{
				if (progressDirection == ProgressDirection.forward){
                	desiredPosition.x -= zSeperation;
				}else{
					desiredPosition.x += zSeperation;
				}
			}
        }
        movement.SetTarget(desiredPosition);
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
