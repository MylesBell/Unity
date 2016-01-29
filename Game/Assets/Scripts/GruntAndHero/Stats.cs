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
    
    // special abilities
    public float fireAttackRadius = 6.0f;
    public float fireAttackDamage = 30.0f;
    public float healRingRadius = 6.0f;
    public float healRingHealth = 30.0f;
    
    
    private int currentKillStreak = 0;
    private object killStreakLock = new object();
    public void ResetKillStreak(){
        lock(killStreakLock){
            currentKillStreak = 0;
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
    }
    
    public void resetFireAttackRadius(){
        fireAttackRadius = 6.0f;
    }
    
    public void resetHealRingRadius(){
        fireAttackRadius = 6.0f;
    }
}