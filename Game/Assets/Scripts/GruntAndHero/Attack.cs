using UnityEngine;
using UnityEngine.Networking;

public class Attack : NetworkBehaviour {

	private GameObject target;
	private float timeTillAttack;
	
	private Stats stats;

    public void initiliseAttack() {
        timeTillAttack = 0;
        target = null;
    }

	void Update () {
        if (isServer) {
            stats = (Stats)GetComponent<Stats>();
            if (target != null && !target.activeSelf) target = null; // check if target is still active, if not then null this
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
		float distance = Vector3.Distance(target.GetComponent<Collider>().ClosestPointOnBounds(transform.position), transform.position);
		
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