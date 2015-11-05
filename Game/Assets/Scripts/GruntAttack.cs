using UnityEngine;
using System.Collections;

public class GruntAttack : MonoBehaviour {
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
				Attack ();
				attackTime = coolDown;
			}
		}
	}
	
	private void Attack() {
		if (targetInAttackArea()){
			((Health)target.GetComponent ("Health")).reduceHealth(20.0f);
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