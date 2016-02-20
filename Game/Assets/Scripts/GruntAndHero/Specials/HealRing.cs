using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HealRing : Special
{   
    public float ringRadius = 6.0f;
    public float healAmount = 30.0f;
    
    override public void InitialiseSpecial()
    {   
        currentScale = new Vector3(1.0f, 1.0f, 0);
        gameObject.transform.parent = gameObject.transform;
    }

    override public void ResetSpecial()
    {
        ringRadius = 6.0f;
        healAmount = 30.0f;
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void UpgradeSpecial()
    {
        currentScale += new Vector3(1.0f, 1.0f, 0);
        ringRadius += 3.0f;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlayHealRingSystem();
        CmdRadialHeal(ringRadius, healAmount);
    }

    [ClientRpc]
    private void RpcPlayHealRingSystem() {
        gameObject.SetActive(true);
        StartCoroutine(PlayHealRingSystem());
    }
    
    private IEnumerator PlayHealRingSystem(){
        // gameObject.transform.rotation = prefab.transform.rotation;
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
    }
    
    [Command]
    private void CmdRadialHeal(float radius, float heal){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            if (CheckColliderWantsToHeal(collider)){
                Health health = (Health)collider.gameObject.GetComponent<Health>();
                if (health.currentHealth + heal > health.maxHealth){
                    health.currentHealth = health.maxHealth;
                }else{
                    health.IncreaseHealth(heal);
                }
            }
        }
    }
    
    private bool CheckColliderWantsToHeal(Collider collider){
        
        // check gameobejct exists first (aka not default)
        if (collider.gameObject.Equals(default(GameObject))) {
            if (collider.gameObject.tag.Equals(specials.ownGruntTag) || collider.gameObject.tag.Equals(specials.ownHeroTag)
              || collider.gameObject.tag.Equals(specials.ownBaseTag)){
                return true;
            }
        }
        
        return false;
    }
}