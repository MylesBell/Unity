using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UnitSlowDown : AllPlay {
    
    private float savedSpeed;
    
    override public void Initialise(){
        
    }
    
    override public void Upgrade(){
        
    }
    
    override public void Reset(){
        
    }
    
    override public void Use(params float[] inputs){   
        float slowDownMultiplier = inputs[0];
        float slowDownTime = inputs[1];
        gameObject.SetActive(true);
        CmdReduceSpeed(slowDownMultiplier);
        RpcPlaySlowDown(slowDownMultiplier, slowDownTime);
    }
    
    [Command]
    private void CmdReduceSpeed(float slowDownMultiplier){
        savedSpeed = stats.movementSpeed;
        stats.movementSpeed = savedSpeed * slowDownMultiplier;
    }
    
    [ClientRpc]
    private void RpcPlaySlowDown(float slowDownMultiplier, float slowDownTime) {
        gameObject.SetActive(true);
        StartCoroutine(PlaySlowDown(slowDownTime));
    }
    
    IEnumerator PlaySlowDown(float slowDownTime){
        yield return new WaitForSeconds(slowDownTime);
        gameObject.SetActive(false);
        stats.movementSpeed = savedSpeed;
    }
}