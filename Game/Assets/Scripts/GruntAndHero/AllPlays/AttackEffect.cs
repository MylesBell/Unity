using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AttackEffect : AllPlay {
        
    override public void Initialise(float height){
        transform.localPosition = new Vector3(0,height,0);
    }
    
    override public void Upgrade(){
        
    }
    
    override public void Reset(){
        
    }
    
    override public void Use(params float[] inputs){   
        gameObject.SetActive(true);
        RpcPlayEffect();
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
    private void RpcPlayEffect() {
        gameObject.SetActive(true);
        StartCoroutine(PlayEffect());
    }
    
    IEnumerator PlayEffect(){
        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
    }
}