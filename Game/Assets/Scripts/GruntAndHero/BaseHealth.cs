using UnityEngine;

public class BaseHealth : Health {
    Team team;
    BaseHealth otherBase;
    
    public void InitialiseHealth(Team team, ComputerLane computerLane) {
        this.team = team;
        GameObject[] bases = GameObject.FindGameObjectsWithTag(gameObject.tag);
        
        foreach (GameObject other in bases) {
            if(other && other != gameObject) {
                otherBase = other.GetComponent<BaseHealth>();
            }
        }
        base.InitialiseHealth(computerLane);
    }
	public new void ReduceHealth(float amountToReduce, out bool killedBase){
		currentHealth -= amountToReduce;
        killedBase = currentHealth < 0;
        damageText.Play(-amountToReduce);
        if(otherBase) otherBase.ChangeFromOtherBase(-amountToReduce);
        team.BaseHealthChange(maxHealth, currentHealth);
	}
    
    public void ChangeFromOtherBase(float amountToReduce){
        currentHealth += amountToReduce;
    }

	public new void IncreaseHealth(float amountToIncrease){
		currentHealth += amountToIncrease;
        damageText.Play(amountToIncrease);
        if(otherBase) otherBase.ChangeFromOtherBase(amountToIncrease);
        team.BaseHealthChange(maxHealth, currentHealth);
	}
}