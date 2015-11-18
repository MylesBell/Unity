using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Attack : NetworkBehaviour {

	private GameObject target;
	private float timeTillAttack;

	// characteristics, move to stats later
	public float damage = 20.0f;
	public float attackCoolDown = 1.0f;
	public float attackRange = 5.0f;

	void Start () {
		timeTillAttack = 0;
		target = null;
	}

	void Update () {
        if (isServer && GameState.gameState == GameState.State.PLAYING) { 
		    if (target != null) {
			    if ((timeTillAttack > 0)) {
				    timeTillAttack -= Time.deltaTime;
			    } else {
				    AttackTarget ();
				    timeTillAttack = attackCoolDown;
			    }
		    }
        }
	}
	
	private void AttackTarget() {
		if (targetInAttackArea()){
			((Health)target.GetComponent ("Health")).reduceHealth(damage);
		}
	}

	private bool targetInAttackArea(){
		float distance = Vector3.Distance (target.transform.position, transform.position);
		
		if (distance < attackRange) {
			return true;
		}
		return false;
	}

	// getters and setters 
	public GameObject getTarget(){
		return target;
	}

	public void setTarget(GameObject newTarget){
		target = newTarget;
	}
}