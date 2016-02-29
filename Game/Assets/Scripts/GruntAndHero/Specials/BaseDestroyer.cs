using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BaseDestroyer : Special
{   
    public float attackDistance = 6.0f;
    public float damageAmount = 500.0f;
    
    override public void InitialiseSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void ResetSpecial()
    {
        attackDistance = 6.0f;
        damageAmount = 500.0f;
        gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void UpgradeSpecial()
    {
        damageAmount += 200.0f;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlayBaseDestroySystem();
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
        GameObject enemyBase = GameObject.FindGameObjectWithTag(specials.attackBaseTag);
        bool killedBase;
        
        if(Vector3.Distance(specials.gameObject.transform.position, enemyBase.transform.position) < attackDistance){
            ((BaseHealth)enemyBase.GetComponent<BaseHealth>()).ReduceHealth(damageAmount, out killedBase);
        }
    }
}