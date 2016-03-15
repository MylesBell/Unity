using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DefenseBuff : Special
{   
    public float defenseIncrease = 0.5f;
    private float originalDefense;
    
    override public void InitialiseSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void ResetSpecial()
    {
        defenseIncrease = 0.5f;
    }

    override public void UpgradeSpecial()
    {
        defenseIncrease += 0.5f;
    }

    override public void UseSpecial()
    {
        originalDefense = stats.damage;
        gameObject.SetActive(true);
        RpcPlayDefenseBuffSystem();
    }
    
    [ClientRpc]
    public void RpcPlayDefenseBuffSystem() {
        stats.damage += defenseIncrease;
        gameObject.SetActive(true);
        StartCoroutine(PlayDefenseBuffSystem());
    }
    
    IEnumerator PlayDefenseBuffSystem(){
        yield return new WaitForSeconds(5.0f);
        gameObject.SetActive(false);
        stats.damage = originalDefense;
    }
}