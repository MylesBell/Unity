using UnityEngine.Networking;

public class Stats : NetworkBehaviour{
	// movement
	public float movementSpeed = 5.0f;
	public float minDistanceFromEnemy = 2.0f;
	
	// attack
	public float damage = 20.0f;
	public float attackCoolDown = 1.0f;
	public float attackRange = 5.0f;
	
	// target select
	public float targetSelectRange = 15.0f;	
    
    public float ignoreRange = 5f;
    public float runAwayTime = 2f;
    public float maximumVelocityBeforeIgnore = 1f;
}