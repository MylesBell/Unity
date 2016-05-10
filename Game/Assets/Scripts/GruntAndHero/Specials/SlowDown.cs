using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SlowDown : Special
{   
    public float slowDownTime = 10.0f;
    public float slowDownRadius = 10.0f;
    private float slowDownMultiplier = 0.8f;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,0,0);
    }

    override public void ResetSpecial()
    {
        slowDownMultiplier = 0.8f;
    }

    override public void UpgradeSpecial()
    {
        slowDownMultiplier -= 0.1f;
        if (slowDownMultiplier < 0) slowDownMultiplier = 0;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlaySlowDownParticleSystem();
        CmdRadialSlowDown(slowDownRadius);
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
    public void RpcPlaySlowDownParticleSystem() {
        gameObject.SetActive(true);
        StartCoroutine(PlaySlowDownRingSystem());
    }
    
    IEnumerator PlaySlowDownRingSystem(){
        yield return new WaitForSeconds(1.0f);
        gameObject.SetActive(false);
    }
    
    // to slow down in circular area about player
    [Command]
    private void CmdRadialSlowDown(float radius){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            if (CheckColliderWantsToAttack(collider)){
                AllPlays allPlays = collider.gameObject.GetComponent<AllPlays>();
                allPlays.SlowDown(new float[2]{slowDownMultiplier, slowDownTime});
            }
        }
    }
    
    private bool CheckColliderWantsToAttack(Collider collider){
        // check gameobejct exists first (aka not default)
        if (!collider.gameObject.Equals(default(GameObject))) {
            return collider.gameObject.tag.Equals(specials.attackGruntTag) || collider.gameObject.tag.Equals(specials.attackHeroTag);
        }
        
        return false;
    }
}