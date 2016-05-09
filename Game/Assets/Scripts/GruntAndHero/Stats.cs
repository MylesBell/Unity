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
    private int kills = 0;
    public int heroKills = 0;
    public int gruntKills = 0;
    public int deaths = 0;
    public int towersCaptured = 0;
    private object killsLock = new object();
    private int firstUpgrade = 1;
    private int nextUpgrade;
    private int level = 1;
    
    private float originalMovementSpeed, originalDamage, originalDefense;
    private bool statsSet;
    
    void Start(){
        ResetStats();
        if (!statsSet) {
            originalMovementSpeed = movementSpeed;
            originalDamage = damage;
            originalDefense = defense;
            statsSet = true;
        }
    }
    
    public void ResetStats(){
        lock(killsLock){
            kills = 0;
            heroKills = 0;
            gruntKills = 0;
            deaths = 0;
            towersCaptured = 0;
            level = 1;
            nextUpgrade = firstUpgrade;
        }
    }

    public void ResetKills(){
        lock(killsLock){
            kills = 0;
        }
    }
    
    public void ResetSpecialStats() {
        movementSpeed = originalMovementSpeed;
        damage = originalDamage;
        defense = originalDefense;
    }
    
    public int GetKills(){
        int val;
        lock(killsLock){
            val = kills;
        }
        return val;
    }
    
    public void IncrementKills(bool heroKill){
        lock(killsLock){
            if (heroKill){
                kills += 5;
                heroKills += 1;
            }else{
                kills++;
                gruntKills++;
            }
            if(gameObject.GetComponent<Hero>() && kills >= nextUpgrade){
                level++;
                SetNextUpgrade();
                gameObject.GetComponent<Specials>().UpgradeSpecials();
                SendUpgradeEvent();
            }
        }
    }
    
    private void SetNextUpgrade(){
        nextUpgrade += (int)(nextUpgrade * 1.5f);
    }
    
    private void SendUpgradeEvent(){
        Hero hero = gameObject.GetComponent<Hero>();
        string playerID = hero.getplayerID();
        SocketIOOutgoingEvents.PlayerLevelUp(playerID, level);
    }
}