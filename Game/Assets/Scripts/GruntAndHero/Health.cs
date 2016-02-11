using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
	public float healthBarInitialLength;
	public Texture healthBarHighTexture, healthBarMedTexture, healthBarLowTexture, healthBarBackTexture;

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
            Texture healthBarTexture = healthBarHighTexture;
            if (currentHealth > 0.6 * maxHealth)
                healthBarTexture = healthBarHighTexture;
            else if (currentHealth > 0.2 * maxHealth)
                healthBarTexture = healthBarMedTexture;
            else healthBarTexture = healthBarLowTexture;
			GUI.DrawTexture(new Rect(entityLocation.x - (healthBarInitialLength/2) - 1,
			                         Screen.height - entityLocation.y - 11,
			                         healthBarInitialLength + 2, 4), healthBarBackTexture);
			GUI.DrawTexture(new Rect(entityLocation.x - (healthBarInitialLength/2) ,
			                         Screen.height - entityLocation.y - 10,
			                         healthBarLength, 2), healthBarTexture);
		}
	}
	
	void Update () {
		entityLocation =  Camera.main.WorldToScreenPoint(gameObject.transform.position);
		if (isServer && currentHealth <= 0) {
            if (gameObject.GetComponent<IDestroyableGameObject>() != null)
                gameObject.GetComponent<IDestroyableGameObject>().DisableGameObject();
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

	public void ReduceHealth(float amountToReduce, out bool killedPlayer){
		currentHealth -= amountToReduce;
        killedPlayer = currentHealth > 0;
        if(gameObject.GetComponent<Hero>() != null) {
            Hero hero = gameObject.GetComponent<Hero>();
            string playerID = hero.getplayerID();
            SocketIOOutgoingEvents.PlayerHealthHasChanged(playerID, -amountToReduce);
        }
	}

	public void IncreaseHealth(float amountToIncrease){
		currentHealth += amountToIncrease;
        if(gameObject.GetComponent<Hero>() != null) {
            Hero hero = gameObject.GetComponent<Hero>();
            string playerID = hero.getplayerID();
            SocketIOOutgoingEvents.PlayerHealthHasChanged(playerID, amountToIncrease);
        }
	}
}