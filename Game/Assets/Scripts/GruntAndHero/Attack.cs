using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Attack : NetworkBehaviour {

	private GameObject target;
	private float timeTillAttack;
	
	private Stats stats;

	void Start () {
		timeTillAttack = 0;
		target = null;
	}

	void Update () {
        if (isServer) {
            stats = (Stats)GetComponent<Stats>();
            if (target != null && GameState.gameState == GameState.State.PLAYING) {
			    if ((timeTillAttack > 0)) {
				    timeTillAttack -= Time.deltaTime;
			    } else {
				    AttackTarget ();
				    timeTillAttack = stats.attackCoolDown;
			    }
		    }
        }
    }
	
	private void AttackTarget() {
		if (targetInAttackArea()){
			((Health)target.GetComponent ("Health")).reduceHealth(stats.damage);
		}
	}

	private bool targetInAttackArea(){
		float distance = Vector3.Distance (target.transform.position, transform.position);
		
		if (distance < stats.attackRange) {
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