using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
	public float healthBarInitialLength;
	public Texture healthBarTexture;

	public float maxHealth;
	[SyncVar] public float currentHealth;
	private float healthBarLength;
	private float percentOfHealth;
	private Vector3 entityLocation;

	void Start(){
        currentHealth = maxHealth;
    }

    public void initialiseHealth() {
        // if max heath isnt set, as can be set by later function
        currentHealth = maxHealth;
        entityLocation = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        percentOfHealth = currentHealth / maxHealth;
        healthBarLength = percentOfHealth * healthBarInitialLength;
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
		if (isServer && currentHealth <= 0) {
            if (gameObject.GetComponent<IDisableGO>() != null)
                gameObject.GetComponent<IDisableGO>().disableGameObject();
			else
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