using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {
	public float healthBarInitialLength;
	public Texture healthBarTexture;

	public float maxHealth;
	private float currentHealth;
	private float healthBarLength;
	private float percentOfHealth;
	private Vector3 entityLocation;

	void Start(){
		// if max heath isnt set, as can be set by later function
		currentHealth = maxHealth;
	}

	void OnGUI () {
		if (currentHealth > 0) {
			GUI.DrawTexture(new Rect(entityLocation.x - (healthBarInitialLength/2) ,
			                         Screen.height - entityLocation.y - 10,
			                         healthBarLength, 2), healthBarTexture);
		}
	}
	
	void Update () {
		entityLocation =  Camera.main.WorldToScreenPoint(gameObject.transform.position);
		if(Input.GetKeyUp (KeyCode.Q)) {
			currentHealth -= 10.0f;
		}
		if (currentHealth <= 0) {
			Destroy(gameObject);
		}
		percentOfHealth = currentHealth / maxHealth;
		healthBarLength = percentOfHealth * healthBarInitialLength;
	}

	// use to set max and inc or dec health
	public void setMaxHealth(float maxHealthInput){
		maxHealth = maxHealthInput;
	}

	public void reduceHealth(float amountToReduce){
		currentHealth -= amountToReduce;
	}

	public void IncreaseHealth(float amountToIncrease){
		currentHealth += amountToIncrease;
	}
}