using UnityEngine;

public class BaseHealth : Health {
    Team team;
    BaseHealth otherBase;
    
    public void initialiseHealth(Team team) {
        this.team = team;
        GameObject[] bases = GameObject.FindGameObjectsWithTag(gameObject.tag);
        
        foreach (GameObject other in bases) {
            if(other && other != gameObject) {
                otherBase = other.GetComponent<BaseHealth>();
            }
        }
        base.initialiseHealth();
    }
	public new void ReduceHealth(float amountToReduce, out bool killedPlayer){
		currentHealth -= amountToReduce;
        killedPlayer = currentHealth > 0;
        if(otherBase) otherBase.ChangeFromOtherBase(-amountToReduce);
        team.BaseHealthChange(maxHealth, currentHealth);
	}
    
    public void ChangeFromOtherBase(float amountToReduce){
        currentHealth += amountToReduce;
    }

	public new void IncreaseHealth(float amountToIncrease){
		currentHealth += amountToIncrease;
        if(otherBase) otherBase.ChangeFromOtherBase(amountToIncrease);
	}
}