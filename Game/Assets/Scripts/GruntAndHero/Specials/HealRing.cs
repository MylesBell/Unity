using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HealRing : Special
{   
    public float ringRadius = 6.0f;
    public float healAmount = 30.0f;
    
    override public void InitialiseSpecial(float height)
    {   
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void ResetSpecial()
    {
        ringRadius = 6.0f;
        healAmount = 30.0f;
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void UpgradeSpecial()
    {
        healAmount += 5f;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlayHealRingSystem();
        CmdRadialHeal(ringRadius, healAmount);
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
    private void RpcPlayHealRingSystem() {
        gameObject.SetActive(true);
        StartCoroutine(PlayHealRingSystem());
    }
    
    private IEnumerator PlayHealRingSystem(){
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
    }
    
    [Command]
    private void CmdRadialHeal(float radius, float heal){
        Collider[] hitColliders = Physics.OverlapSphere(transform.parent.position, radius);
        foreach(Collider collider in hitColliders) {
            bool isNotGrunt;
            if (CheckColliderWantsToHeal(collider, out isNotGrunt)){
                Health health = (Health)collider.gameObject.GetComponent<Health>();
                heal = isNotGrunt? 2 * heal : heal;
                if (health.currentHealth + heal > health.maxHealth){
                    health.currentHealth = health.maxHealth;
                }else{
                    health.IncreaseHealth(heal);
                }
            }
        }
    }
    
    private bool CheckColliderWantsToHeal(Collider collider, out bool isNotGrunt){
        
        // check gameobejct exists first (aka not default)
        if (!collider.gameObject.Equals(default(GameObject))) {
            if (collider.gameObject.tag.Equals(specials.ownGruntTag)){
               isNotGrunt = false;
               return true;
            } else if (collider.gameObject.tag.Equals(specials.ownHeroTag)
              || collider.gameObject.tag.Equals(specials.ownBaseTag)){
                isNotGrunt = true;
                return true;
            }
        }
        isNotGrunt = false;
        return false;
    }
}