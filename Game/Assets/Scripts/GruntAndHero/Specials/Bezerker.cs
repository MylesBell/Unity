using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Bezerker : Special
{   
    public GameObject model;
    private float ringRadius = 10.0f;
    private float damageAmount = 200.0f;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,height/2,0);
        RpcFindModels();
    }

    override public void ResetSpecial()
    {
        damageAmount = 200f;
    }

    override public void UpgradeSpecial()
    {
        damageAmount += 40f;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlayBezerkerSystem();
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
    public void RpcFindModels() {
        foreach (Transform child in transform.parent) {
            if (child.tag == "originalModel")
                model = child.gameObject;
        }
    }
    
    [ClientRpc]
    public void RpcPlayBezerkerSystem() {     
        gameObject.SetActive(true);
        StartCoroutine(PlayBezerkerSystem());
    }
    
    IEnumerator PlayBezerkerSystem() {
        float originalDamage = stats.damage;
        stats.damage *= 2;
        Health health = GetComponentInParent<Health>();
        TargetSelect target = GetComponentInParent<TargetSelect>();
        ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
        yield return new WaitForSeconds(4.0f);
        particleSystem.Play();
        RadialDamage(ringRadius, damageAmount);
        stats.damage = originalDamage;
        yield return new WaitForSeconds(1.0f);
        bool killedSelf;
        if (isServer)
            health.ReduceHealth(health.maxHealth, out killedSelf);
        gameObject.SetActive(false);
        particleSystem.Stop();
    }
  
    
    // to deal damage and heal in a circular area about player
    private void RadialDamage(float radius, float damage){
        // Cannot call RPC command from IEnumerator, reverting to old isServer method
        if (isServer) {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
            foreach(Collider collider in hitColliders) {
                if (CheckColliderWantsToAttack(collider)){
                    bool killedObject;
                    if (collider.gameObject.tag.Equals(specials.attackBaseTag)){
                        collider.gameObject.GetComponent<BaseHealth>().ReduceHealth(damage, out killedObject);
                    }else{
                        ((Health)collider.gameObject.GetComponent<Health>()).ReduceHealth(damage, out killedObject);
                    }
                    if(killedObject){
                        stats.IncrementKills(collider.gameObject.GetComponent<Hero>() != null);
                    }
                }
            }
        }
    }
    
    private bool CheckColliderWantsToAttack(Collider collider){
        // check gameobejct exists first (aka not default)
        if (!collider.gameObject.Equals(default(GameObject))) {
            return collider.gameObject.tag.Equals(specials.attackGruntTag) || collider.gameObject.tag.Equals(specials.attackHeroTag)
                || collider.gameObject.tag.Equals(specials.attackBaseTag);
        }
        return false;
    }
}