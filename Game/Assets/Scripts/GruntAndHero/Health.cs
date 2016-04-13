using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
	public float healthBarInitialLength;
	public Texture healthBarHighTexture, healthBarMedTexture, healthBarLowTexture, healthBarBackTexture;
    
    public float healthBarOffset = 0.0f;
	public float maxHealth;
	[SyncVar] public float currentHealth;
	private float healthBarLength;
	private float percentOfHealth;
	private Vector3 entityLocation;
    
    protected DamageText damageText;
    
	void Start(){
        currentHealth = maxHealth;
    }
    
    public virtual void InitialiseHealth(ComputerLane computerlane) {
        // if max heath isnt set, as can be set by later function
        currentHealth = maxHealth;
        entityLocation = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        percentOfHealth = currentHealth / maxHealth;
        healthBarLength = percentOfHealth * healthBarInitialLength;
        
        damageText = gameObject.GetComponent<DamageText>();
        damageText.InitialiseDamageText(computerlane);
    }

	void OnGUI () {
		if (currentHealth > 0) {
            Texture healthBarTexture = healthBarHighTexture;
            if (currentHealth > 0.6 * maxHealth)
                healthBarTexture = healthBarHighTexture;
            else if (currentHealth > 0.2 * maxHealth)
                healthBarTexture = healthBarMedTexture;
            else healthBarTexture = healthBarLowTexture;
            int healthBarHeight = (Screen.height / 150) < 3? 3 : Screen.height / 150;
            healthBarHeight -= healthBarHeight % 3;
            float length = healthBarInitialLength * healthBarHeight + (2 * healthBarHeight/3);
            float height = healthBarHeight + (2 * healthBarHeight / 3);
            float yOffset = healthBarOffset * height;
            float xPos = entityLocation.x - (length/2) - (healthBarHeight / 3);
            float yPos = Screen.height - entityLocation.y - yOffset - (healthBarHeight / 3);
			GUI.DrawTexture(new Rect(xPos, yPos,length, height), healthBarBackTexture);
			GUI.DrawTexture(new Rect(entityLocation.x - (length/2) ,
			                         Screen.height - entityLocation.y - yOffset,
			                         healthBarLength * healthBarHeight, healthBarHeight), 
                                     healthBarTexture);
		}
	}
	
	void Update () {
		entityLocation =  Camera.main.WorldToScreenPoint(gameObject.transform.position);
		if (isServer && currentHealth <= 0) {
            if (gameObject.GetComponent<IDestroyableGameObject>() != null){
                gameObject.GetComponent<IDestroyableGameObject>().DisableGameObject();
            }else{
                Destroy(gameObject);
            }
		}
		percentOfHealth = currentHealth / maxHealth;
		healthBarLength = percentOfHealth * healthBarInitialLength;
	}

	// use to set max and inc or dec health
	public void SetMaxHealth(float maxHealthInput){
		maxHealth = maxHealthInput;
	}

	public void ReduceHealth(float amountToReduce, out bool killedPlayer){
		currentHealth -= amountToReduce;
        killedPlayer = currentHealth < 0;
        if (!killedPlayer) damageText.Play(-amountToReduce);
        
        if(gameObject.GetComponent<Hero>() != null) {
            Hero hero = gameObject.GetComponent<Hero>();
            string playerID = hero.getplayerID();
            SocketIOOutgoingEvents.PlayerHealthHasChanged(playerID, -amountToReduce);
        }
	}

	public void IncreaseHealth(float amountToIncrease){
		currentHealth += amountToIncrease;
        damageText.Play(amountToIncrease);
        
        if(gameObject.GetComponent<Hero>() != null) {
            Hero hero = gameObject.GetComponent<Hero>();
            string playerID = hero.getplayerID();
            SocketIOOutgoingEvents.PlayerHealthHasChanged(playerID, amountToIncrease);
        }
	}
}