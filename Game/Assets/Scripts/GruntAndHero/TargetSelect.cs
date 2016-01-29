using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TargetSelect : NetworkBehaviour {

	public TeamID teamID;
	private Attack attack;
	private Movement movement;
	private float desiredZPosition;
	private Vector3 desiredPosition;
	private float zSeperation;
	private ProgressDirection progressDirection;

    public string attackGruntTag;
    public string attackHeroTag;
    public string attackBaseTag;
    private string homeBaseTag;
    private Stats stats;
    
    private bool nearBaseLast;
    
    private Dictionary<GameObject, float> collidersToIgnore = new Dictionary<GameObject, float>();
	
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
		movement = GetComponent<Movement>();
		movement.SetTarget (desiredPosition);
        attackGruntTag = teamID == TeamID.blue ? "redGrunt" : "blueGrunt";
        attackHeroTag = teamID == TeamID.blue ? "redHero" : "blueHero";
        attackBaseTag = teamID == TeamID.blue ? "redBase" : "blueBase";
        homeBaseTag = teamID == TeamID.blue ? "blueBase" : "redBase";
        nearBaseLast = false;
    }
	
	public void SetProgressDirection(ProgressDirection progressDirection){
		this.progressDirection = progressDirection;
        desiredPosition = transform.position;
        if(hasAttackTarget() && GetComponent<Rigidbody>().velocity.magnitude < stats.maximumVelocityBeforeIgnore) findAndIgnoreCloseColliders();
	}

	public void MoveToZOffset(MoveDirection moveDirection, float maxZ, float minZ){
        switch (moveDirection) {
            case MoveDirection.up:
                desiredZPosition = ((desiredZPosition + zSeperation) < maxZ) ? desiredZPosition + zSeperation : maxZ; 
                break;
            case MoveDirection.down:
                desiredZPosition = ((desiredZPosition - zSeperation) > minZ) ? desiredZPosition - zSeperation : minZ; 
                break;
        }
        desiredPosition = new Vector3(transform.position.x, transform.position.y, desiredZPosition);
        movement.SetTarget(desiredPosition);
        if(hasAttackTarget() && GetComponent<Rigidbody>().velocity.magnitude < stats.maximumVelocityBeforeIgnore) findAndIgnoreCloseColliders();
	}

	void Update () {
        if (isServer) {
            bool nearBaseCurrent;
            attack.setTarget(GetNewAttackTarget(out nearBaseCurrent));
            
            //do movement
            if (hasAttackTarget()) {
                movement.SetTarget(attack.getTarget().GetComponent<Collider>().ClosestPointOnBounds(transform.position));
            } else {
                UpdateMoveTarget();
            }
            
            //do near base event for heros only
            if(GetComponent<Hero>() && nearBaseCurrent != nearBaseLast) SocketIOOutgoingEvents.PlayerNearBase (GetComponent<Hero>().getplayerID(), nearBaseCurrent);
            nearBaseLast = nearBaseCurrent;
        }
        foreach(GameObject key in collidersToIgnore.Keys) {
            collidersToIgnore[key] -= Time.deltaTime;
            if(collidersToIgnore[key] < 0) collidersToIgnore.Remove(key);
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

	private GameObject GetNewAttackTarget(out bool nearHomeBase){
        Collider closestBase = null;
        Collider closestHero = null;
        Collider closestGrunt = null;
        float currentDistanceBase = Mathf.Infinity;
        float currentDistanceHero = Mathf.Infinity;
        float currentDistanceGrunt = Mathf.Infinity;
        nearHomeBase = false;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stats.targetSelectRange);
        foreach(Collider collider in hitColliders) {
            if (collider.gameObject.activeSelf && !collidersToIgnore.ContainsKey(collider.gameObject)) { //check if active
                if (string.Equals(collider.gameObject.tag, attackGruntTag)) {
                    closestGrunt = closestCollider(closestGrunt, collider, ref currentDistanceGrunt);
                } else if (string.Equals(collider.gameObject.tag, attackHeroTag)) {
                    closestHero = closestCollider(closestHero, collider, ref currentDistanceHero);
                } else if (string.Equals(collider.gameObject.tag, attackBaseTag)) {
                    closestBase = closestCollider(closestBase, collider, ref currentDistanceBase);
                } else if (string.Equals(collider.gameObject.tag, homeBaseTag)) {
                    nearHomeBase = true;
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
    
    private void findAndIgnoreCloseColliders(){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stats.ignoreRange);
        foreach(Collider collider in hitColliders) {
            if (collider.gameObject.activeSelf) { //check if active
                if (string.Equals(collider.gameObject.tag, attackGruntTag)
                    || string.Equals(collider.gameObject.tag, attackHeroTag)
                    || string.Equals(collider.gameObject.tag, attackBaseTag)) {
                    collidersToIgnore.Add(collider.gameObject, stats.runAwayTime);
                }
            }
        }
    }
}
