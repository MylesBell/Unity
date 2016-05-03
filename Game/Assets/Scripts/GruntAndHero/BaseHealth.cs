using UnityEngine;

public class BaseHealth : Health {
    Team team;
    BaseHealth otherBase;
    ComputerLane computerLane;
    
    public void InitialiseBaseHealth(Team team, ComputerLane computerLane) {
        this.team = team;
        GameObject[] bases = GameObject.FindGameObjectsWithTag(gameObject.tag);
        
        foreach (GameObject other in bases) {
            if(other && other != gameObject) {
                otherBase = other.GetComponent<BaseHealth>();
            }
        }
        this.computerLane = computerLane;
        InitialiseHealth(computerLane);
    }
	public new void ReduceHealth(float amountToReduce, out bool killedBase){
        if(currentHealth > 0) {
            currentHealth -= amountToReduce;
            killedBase = currentHealth <= 0;
            if (killedBase) {
                gameObject.GetComponent<IDestroyableGameObject>().DisableGameObject();
                otherBase.gameObject.GetComponent<IDestroyableGameObject>().DisableGameObject();
            }
            damageText.Play(-amountToReduce);
            if(otherBase) otherBase.ChangeFromOtherBase(-amountToReduce);
            team.BaseHealthChange(maxHealth, currentHealth);
        } else {
            killedBase = false;
        }
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
    
    public ComputerLane getComputerLane(){
        return this.computerLane;
    }
}