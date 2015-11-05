using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {
	public float damage = 20.0f;
	private GameObject target;
	private float attackTime;
	private float coolDown;

	void Start () {
		attackTime = 0;
		coolDown = 1.0f;
		target = null;
	}

	void Update () {
		if (target != null) {
			if ((attackTime > 0)) {
				attackTime -= Time.deltaTime;
			} else {
				AttackTarget ();
				attackTime = coolDown;
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
		
		if (distance < 5.0f) {
			return true;
		}
		return false;
	}

	public void setTarget(GameObject newTarget){
		target = newTarget;
	}
}