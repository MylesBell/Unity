using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UnitAttackIncrease : AllPlay {
    
    private float originalAttack;
    
    override public void Initialise(float height){
        transform.localPosition = new Vector3(0,height,0);
    }
    
    override public void Upgrade(){
        
    }
    
    override public void Reset(){
        
    }
    
    override public void Use(params float[] inputs){   
        float attackIncrease = inputs[0];
        float attackIncreaseTime = inputs[1];
        gameObject.SetActive(true);
        CmdIncreaseAttack(attackIncrease);
        RpcPlayAttackIncrease(attackIncrease, attackIncreaseTime);
    }
    
    override public void Kill(){
        RpcKill();
    }
    
    [ClientRpc]
    private void RpcKill() {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }    
    
    [Command]
    private void CmdIncreaseAttack(float attackIncrease){
        originalAttack = stats.damage;
        stats.damage += attackIncrease;
    }
    
    [ClientRpc]
    private void RpcPlayAttackIncrease(float attackIncrease, float attackIncreaseTime) {
        gameObject.SetActive(true);
        StartCoroutine(PlayAttackIncrease(attackIncreaseTime));
    }
    
    IEnumerator PlayAttackIncrease(float attackIncreaseTime){
        yield return new WaitForSeconds(attackIncreaseTime);
        gameObject.SetActive(false);
        if (isServer)
            stats.damage = originalAttack;
    }
}