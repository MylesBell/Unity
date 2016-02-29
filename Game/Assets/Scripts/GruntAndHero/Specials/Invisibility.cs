using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Invisibility : Special
{   
    public float invisibilityTime = 10.0f;
    
    override public void InitialiseSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void ResetSpecial()
    {
        invisibilityTime = 10.0f;
    }

    override public void UpgradeSpecial()
    {
        invisibilityTime += 5.0f;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlayInvisibiltySystem();
    }
    
    [ClientRpc]
    public void RpcPlayInvisibiltySystem() {
        CmdSetNotAttackable();
        gameObject.SetActive(true);
        StartCoroutine(PlayInvisibiltySystem());
    }
    
    IEnumerator PlayInvisibiltySystem(){
        yield return new WaitForSeconds(invisibilityTime);
        gameObject.SetActive(false);
        setAttackable();
    }
    
    [Command]
    private void CmdSetNotAttackable(){
       // get all enemy grunts
       GameObject[] enemyGrunts = GameObject.FindGameObjectsWithTag(specials.attackGruntTag);
       
       // for each set to not attack this hero
       foreach(GameObject enemy in enemyGrunts) {
           ((TargetSelect) enemy.GetComponent<TargetSelect>()).setNotAttackable(specials.gameObject);
       }
    }
    
    private void setAttackable(){
        // get all enemy grunts
       GameObject[] enemyGrunts = GameObject.FindGameObjectsWithTag(specials.attackGruntTag);
       
       // for each set to attack this hero again
       foreach(GameObject enemy in enemyGrunts) {
           ((TargetSelect) enemy.GetComponent<TargetSelect>()).setAttackable(specials.gameObject);
       }
    }
}