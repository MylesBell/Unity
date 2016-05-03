using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Bezerker : Special
{   
    public GameObject model;
    public float ringRadius = 8.0f;
    public float damageAmount = 200.0f;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,height/2,0);
        RpcFindModels();
    }

    override public void ResetSpecial()
    {
        
    }

    override public void UpgradeSpecial()
    {
        
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
        Renderer renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
        foreach (Material material in renderer.materials) {
            Color color = Color.black;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color);
        }
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
        Renderer renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
        Health health = GetComponentInParent<Health>();
        TargetSelect target = GetComponentInParent<TargetSelect>();
        ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
        Debug.Log(renderer.materials.Length);
        foreach (Material material in renderer.materials) {
            Color color = target.teamID == TeamID.red ? Color.red : Color.blue;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color);
        }
        yield return new WaitForSeconds(4.0f);
        particleSystem.Play();
        RadialDamage(ringRadius, damageAmount);
        stats.damage = originalDamage;
        yield return new WaitForSeconds(1.0f);
        bool killedSelf;
        if (isServer)
            health.ReduceHealth(health.maxHealth, out killedSelf);
        foreach (Material material in renderer.materials) {
            Color color = Color.black;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color);
        }
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
                    if(killedObject) stats.IncrementKillStreak();
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