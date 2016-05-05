using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UnitDefenceIncrease : AllPlay {
    
    private float originalDefence;
    
    override public void Initialise(float height){
        transform.localPosition = new Vector3(0,height,0);
    }
    
    override public void Upgrade(){
        
    }
    
    override public void Reset(){
        
    }
    
    override public void Use(params float[] inputs){   
        float defenceIncrease = inputs[0];
        float defenceIncreaseTime = inputs[1];
        gameObject.SetActive(true);
        CmdIncreaseDefence(defenceIncrease);
        RpcPlayDefenceIncrease(defenceIncrease, defenceIncreaseTime);
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
    private void CmdIncreaseDefence(float defenceIncrease){
        originalDefence = GetStats().defense;
        GetStats().defense += defenceIncrease;
    }
    
    [ClientRpc]
    private void RpcPlayDefenceIncrease(float defenceIncrease, float defenceIncreaseTime) {
        gameObject.SetActive(true);
        StartCoroutine(PlayDefenceIncrease(defenceIncreaseTime));
    }
    
    IEnumerator PlayDefenceIncrease(float defenceIncreaseTime){
        yield return new WaitForSeconds(defenceIncreaseTime);
        gameObject.SetActive(false);
        if (isServer)
            stats.defense = originalDefence;
    }
}