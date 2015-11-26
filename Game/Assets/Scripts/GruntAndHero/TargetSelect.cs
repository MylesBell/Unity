using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TargetSelect : NetworkBehaviour {

	private TeamID teamID;
	private Attack attack;
	private Movement movement;
	private float desiredZPosition;
	private Vector3 desiredPosition;
	private float zSeperation;
	
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
		attack = GetComponent<Attack> ();
		movement = GetComponent<Movement> ();
		movement.SetTarget (desiredPosition);
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
                movement.SetTarget(attack.getTarget().transform.position);
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
                desiredPosition.x += zSeperation;
			}else{
                desiredPosition.x -= zSeperation;
			}
            movement.SetTarget(desiredPosition);
		}
	}

	private bool hasAttackTarget(){
		return (attack.getTarget () != null);
	}

	private GameObject GetNewAttackTarget(){
		GameObject attackTarget;
		// targets entities in priority order
		attackTarget = GetClosestEnemyGrunt ();
		if (attackTarget == null) {
			attackTarget = GetClosestEnemyHero();
		}
		if (attackTarget == null) {
			attackTarget = GetEnemyBase();
		}
		return attackTarget;
	}
	
	private GameObject GetClosestEnemyGrunt(){
		GameObject attackTarget = null;
		if (teamID == TeamID.blue) {
			attackTarget = FindClosestInRange("redGrunt");
		} else {
			attackTarget = FindClosestInRange("blueGrunt");
		}
		return attackTarget;
	}
	
	private GameObject GetClosestEnemyHero(){
		GameObject attackTarget = null;
		if (teamID == TeamID.blue) {
			attackTarget = FindClosestInRange("redHero");
		} else {
			attackTarget = FindClosestInRange("blueHero");
		}
		return attackTarget;
	}
	
	private GameObject GetEnemyBase(){
		GameObject attackTarget = null;
		if (teamID == TeamID.blue) {
			attackTarget = FindClosestInRange("redBase");
		} else {
			attackTarget = FindClosestInRange("blueBase");
		}
		return attackTarget;
	}

	private GameObject FindClosestInRange(string type){
		GameObject attackTarget = FindClosestObjectWithTag (type);

		if (attackTarget == null) {
			return attackTarget;
		}

		if (TargetInRange (attackTarget)) {
			return attackTarget;
		}

		return null;
	}

	private bool TargetInRange(GameObject attackTarget){
		return (Vector3.Distance (attackTarget.transform.position, transform.position) < stats.targetSelectRange);
	}

	private GameObject FindClosestObjectWithTag(string type) {
		GameObject[] gos = GameObject.FindGameObjectsWithTag(type);
		GameObject closest = null;
		float distance = Mathf.Infinity;
		Vector3 position = transform.position;
		foreach (GameObject go in gos) {
			Vector3 diff = go.transform.position - position;
			float curDistance = diff.sqrMagnitude;
			if (curDistance < distance) {
				closest = go;
				distance = curDistance;
			}
		}
		return closest;
	}
}
