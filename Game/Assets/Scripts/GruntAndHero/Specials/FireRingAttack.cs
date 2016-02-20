using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FireRingAttack : Special
{   
    public float ringRadius = 6.0f;
    public float damageAmount = 30.0f;
    
    override public void InitialiseSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void ResetSpecial()
    {
        ringRadius = 6.0f;
        damageAmount = 30.0f;
        gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void UpgradeSpecial()
    {
        // to increase size of fire particle attack the scale is increased, then the damage area is incremented
        currentScale += new Vector3(1.0f, 1.0f, 0);
        ringRadius += 3.0f;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlayFireParticleSystem();
        CmdRadialDamage(ringRadius, damageAmount);
    }
    
    [ClientRpc]
    public void RpcPlayFireParticleSystem() {
        gameObject.SetActive(true);
        StartCoroutine(PlayFireRingSystem());
    }
    
    IEnumerator PlayFireRingSystem(){
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
    }
    
    // to deal damage and heal in a circular area about player
    [Command]
    private void CmdRadialDamage(float radius, float damage){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            if (CheckColliderWantsToAttack(collider)){
                bool killedObject;
                ((Health)collider.gameObject.GetComponent<Health>()).ReduceHealth(damage, out killedObject);
                if(killedObject) stats.IncrementKillStreak();
            }
        }
    }
    
    private bool CheckColliderWantsToAttack(Collider collider){
        
        if (collider.gameObject.tag.Equals(specials.attackGruntTag) || collider.gameObject.tag.Equals(specials.attackHeroTag)
            || collider.gameObject.tag.Equals(specials.attackBaseTag)){
            return true;
        }
        return false;
    }
}