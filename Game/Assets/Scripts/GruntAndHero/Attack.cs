using UnityEngine;
using UnityEngine.Networking;

public class Attack : NetworkBehaviour {

	private GameObject target;
	public Animator animator;

	private float timeTillAttack;
	private bool isAttacking = true;
	private Stats stats;

    public void initiliseAttack() {
		timeTillAttack = 0;
		target = null;
		isAttacking = true;
		CmdSetAttacking(false);
    }
	public void SwitchAnimator(Animator anim) {
        animator = anim;
    }

	void Update () {
        if (isServer) {
            stats = (Stats)GetComponent<Stats>();
            if (target && !target.activeSelf) target = null; // check if target is still active, if not then null this
            if (target && GameState.gameState == GameState.State.PLAYING && gameObject.GetComponent<Health>().currentHealth > 0) {
			    if ((timeTillAttack > 0)) {
				    timeTillAttack -= Time.deltaTime;
			    } else {
				    AttackTarget ();
				    timeTillAttack = stats.attackCoolDown;
			    }
		    }else if(!target && GameState.gameState == GameState.State.PLAYING){
				CmdSetAttacking(false);
			}
        }
    }
	
	private void AttackTarget() {
		if (targetInAttackArea()){
			CmdSetAttacking(true);
            bool killedObject;
            if(target.GetComponent<BaseHealth>()){
                target.GetComponent<BaseHealth>().ReduceHealth(stats.damage + stats.GetKills()/10, out killedObject);
            }else{
                // attack based on hero damage, kills and enemy defense
                float damageToDo = (stats.damage + stats.GetKills()/10)/target.GetComponent<Stats>().defense;
                target.GetComponent<Health>().ReduceHealth(damageToDo, out killedObject);
            }
            if(killedObject) {
				stats.IncrementKills(target.GetComponent<Hero>() != null);
			}
		} else {
			CmdSetAttacking(false);
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
	
	[Command]
	public void CmdSetAttacking(bool attacking) {
		if (isAttacking != attacking){
			RpcSetAttacking(attacking);
			isAttacking = attacking;
		}
	}
	
	[ClientRpc]
	public void RpcSetAttacking(bool attacking) {
		animator.SetBool("Attacking", attacking);
		if (gameObject.GetComponent<Hero>()){
			TrailRenderEnabled(attacking);
		}
	}
	
	private void TrailRenderEnabled(bool enabled){
		TrailRenderer[] trailRenderers = gameObject.GetComponentsInChildren<TrailRenderer>();
		foreach (TrailRenderer trailRenderer in trailRenderers){
			trailRenderer.enabled = enabled;
		}
	}
}