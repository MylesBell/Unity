using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BombDrop : Special
{   
    public Material original,flash;
    public float ringRadius = 6.0f;
    public float damageAmount = 100.0f;
    private Transform parentTransform;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        RpcSetParentTransform();
    }

    override public void ResetSpecial()
    {
        ringRadius = 6.0f;
        damageAmount = 100.0f;
        gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void UpgradeSpecial()
    {
        damageAmount += 20f;
    }

    override public void UseSpecial()
    {
        transform.parent = null;
        transform.position = parentTransform.position + new Vector3(1,0,0);
        gameObject.SetActive(true);
        RpcPlayBombSystem();
    }
    
    override public void Kill(){
        RpcKill();
    }
    
    [ClientRpc]
    private void RpcKill() {
        StopAllCoroutines();
        transform.parent = parentTransform;
        gameObject.SetActive(false);
    }
    
    
    [ClientRpc]
    public void RpcPlayBombSystem() {
        transform.parent = null;        
        transform.position = parentTransform.position + new Vector3(1,0,0);        
        gameObject.SetActive(true);
        StartCoroutine(PlayBombSystem());
    }
    
    [ClientRpc]
    public void RpcSetParentTransform() {
        parentTransform = transform.parent;
    }
    
    IEnumerator PlayBombSystem() {
        Renderer renderer = GetComponent<Renderer>();
        ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
        for (int i = 11; i >= 4; i--) {
            renderer.material = flash;
            yield return new WaitForSeconds(0.1f);
            renderer.material = original;
            yield return new WaitForSeconds((0.02f*i));
        }
        particleSystem.Play();
        renderer.enabled = false;
        RadialDamage(ringRadius, damageAmount);        
        yield return new WaitForSeconds(1.0f);
        gameObject.SetActive(false);
        particleSystem.Stop();        
        renderer.enabled = true;
        transform.parent = parentTransform;
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
                        collider.gameObject.GetComponent<BaseHealth>().ReduceHealth(2*damage, out killedObject);
                    }else{
                        ((Health)collider.gameObject.GetComponent<Health>()).ReduceHealth(damage, out killedObject);
                    }
                    if(killedObject){
                        if (collider.gameObject.GetComponent<Hero>()){
                            stats.IncrementKills(true);
                        }else{
                            stats.IncrementKills(false);
                        }
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