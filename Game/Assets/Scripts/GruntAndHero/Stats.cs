using UnityEngine;
using UnityEngine.Networking;

public class Stats : NetworkBehaviour{
	// movement
	public float movementSpeed = 5.0f;
    public float movementAcceleration = 8.0f;
	public float minDistanceFromEnemy = 2.0f;
	
	// attack
	public float damage = 20.0f;
	public float attackCoolDown = 1.0f;
	public float attackRange = 5.0f;
    
    // defense
    public float defense = 1.0f;
	
	// target select
	public float targetSelectRange = 15.0f;	
    public float ignoreRange = 5f;
    public float runAwayTime = 2f;
    public float maximumVelocityBeforeIgnore = 1f;
        
    // kill streaks
    private int currentKillStreak = 0;
    private object killStreakLock = new object();
    public int firstUpgrade = 1;
    private int nextUpgrade;
    
    void Start(){
        nextUpgrade = firstUpgrade;
    }
    
    public void ResetKillStreak(){
        lock(killStreakLock){
            currentKillStreak = 0;
            nextUpgrade = firstUpgrade;
        }
    }
    
    public int GetKillStreak(){
        int val;
        lock(killStreakLock){
            val = currentKillStreak;
        }
        return val;
    }
    
    public void IncrementKillStreak(){
        lock(killStreakLock){
            currentKillStreak++;
        }
        if(gameObject.GetComponent<Hero>() && currentKillStreak >= nextUpgrade){
            Debug.Log("UPGRADE");
            gameObject.GetComponent<Specials>().UpgradeSpecials();
            nextUpgrade = nextUpgrade * 2;
        }
    }
}