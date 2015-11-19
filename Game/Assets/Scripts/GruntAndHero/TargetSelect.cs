using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TargetSelect : NetworkBehaviour {

	private TeamID teamID;
	private Attack attack;
	private Movement movement;
	private Channel channel;
	private Vector3 channelTarget;
	private float channelOffset;
	
	private Stats stats;
	
	void Start() {
		stats = (Stats) GetComponent<Stats>();
	}
	
	public void InitialiseTargetSelect (TeamID teamIDInput, Channel channelInput, Vector3 channelTargetInput, float channelOffsetInput)
	{
		teamID = teamIDInput;
		channel = channelInput;
		channelTarget = channelTargetInput;
		channelOffset = channelOffsetInput;
		attack = GetComponent<Attack> ();
		movement = GetComponent<Movement> ();
		movement.SetTarget (channelTarget);
	}

	public void MoveToChannel(Channel channelInput){

		if (channel != channelInput){
			channel = channelInput;
			if (channelInput == Channel.up){
				channelTarget = channelTarget + new Vector3(0,0,2*channelOffset);
			}else{
				channelTarget = channelTarget - new Vector3(0,0,2*channelOffset);
			}      
			movement.SetTarget (channelTarget);
		}

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
                UpdateChannelMoveTarget();
            }
        }
	}
	
	private void UpdateChannelMoveTarget(){
		Vector3 currentTarget = movement.GetTarget ();
		float distance = Vector3.Distance (currentTarget, transform.position);
		
		if (distance < 2.0f) {
			if (teamID == TeamID.blue){
				channelTarget += new Vector3(channelOffset,0,0);
			}else{
				channelTarget -= new Vector3(channelOffset,0,0);
			}
			movement.SetTarget(channelTarget);
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
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag(type);
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
