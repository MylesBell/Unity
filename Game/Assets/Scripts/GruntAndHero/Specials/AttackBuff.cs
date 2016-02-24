using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AttackBuff : Special
{   
    public float attackIncrease = 10.0f;
    private float originalAttack;
    
    override public void InitialiseSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void ResetSpecial()
    {
        attackIncrease = 10.0f;
    }

    override public void UpgradeSpecial()
    {
        attackIncrease += 10.0f;
    }

    override public void UseSpecial()
    {
        originalAttack = stats.damage;
        gameObject.SetActive(true);
        RpcPlaySpeedBuffSystem();
    }
    
    [ClientRpc]
    public void RpcPlaySpeedBuffSystem() {
        stats.damage += attackIncrease;
        gameObject.SetActive(true);
        StartCoroutine(PlaySpeedBuffSystem());
    }
    
    IEnumerator PlaySpeedBuffSystem(){
        yield return new WaitForSeconds(5.0f);
        gameObject.SetActive(false);
        stats.damage = originalAttack;
    }
}