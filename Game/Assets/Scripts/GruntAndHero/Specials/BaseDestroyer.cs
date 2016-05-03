using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BaseDestroyer : Special
{   
    public float attackDistance = 10.0f;
    public float damageAmount = 1000.0f;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,height,0);
    }

    override public void ResetSpecial()
    {
        attackDistance = 10.0f;
        damageAmount = 500.0f;
        gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void UpgradeSpecial()
    {
        damageAmount += 200.0f;
    }

    override public void UseSpecial()
    {
        CmdDamageBase();
        gameObject.SetActive(true);
        RpcPlayBaseDestroySystem();
    }
    
    override public void Kill(){
        RpcKill();
    }
    
    [ClientRpc]
    private void RpcKill() {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
    
    [ClientRpc]
    public void RpcPlayBaseDestroySystem() {
        gameObject.SetActive(true);
        StartCoroutine(PlayBaseDestroySystem());
    }
    
    IEnumerator PlayBaseDestroySystem(){
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
    }
    
    [Command]
    private void CmdDamageBase(){
        GameObject enemyBase;
        float distanceToBase;
        
        closestEnemyBase(out enemyBase, out distanceToBase);
        
        if(distanceToBase < attackDistance){
            bool killedBase;
            ((BaseHealth)enemyBase.GetComponent<BaseHealth>()).ReduceHealth(damageAmount, out killedBase);
        }
    }
    
    private void closestEnemyBase(out GameObject targetBase, out float distanceToBase){
        GameObject[] enemyBases = GameObject.FindGameObjectsWithTag(specials.attackBaseTag);
        
        targetBase = null;
        distanceToBase = float.PositiveInfinity;
        float tempDistanceToBase = float.PositiveInfinity;
        foreach (GameObject enemyBase in enemyBases){
            tempDistanceToBase = Vector3.Distance(
                enemyBase.GetComponent<Collider>().ClosestPointOnBounds(specials.gameObject.transform.position),
                specials.gameObject.transform.position);
            if (tempDistanceToBase < distanceToBase){
                targetBase = enemyBase;
                distanceToBase = tempDistanceToBase;
            }
        }
    }
}