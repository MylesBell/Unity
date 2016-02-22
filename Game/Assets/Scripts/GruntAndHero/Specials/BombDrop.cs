using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BombDrop : Special
{   
    public Material original,flash;
    public float ringRadius = 6.0f;
    public float damageAmount = 80.0f;
    private Transform parentTransform;
    
    override public void InitialiseSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        parentTransform = transform.parent;
    }

    override public void ResetSpecial()
    {
        ringRadius = 6.0f;
        damageAmount = 80.0f;
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
        transform.parent = null;
        transform.position = parentTransform.position + new Vector3(1,0,0);
        gameObject.SetActive(true);
        RpcPlayBombSystem();
    }
    
    
    [ClientRpc]
    public void RpcPlayBombSystem() {
        transform.parent = null;        
        gameObject.SetActive(true);
        StartCoroutine(PlayBombSystem());
    }
    
    IEnumerator PlayBombSystem() {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
        for (int i = 0; i < 20; i++) {
            renderer.material = flash;
            yield return new WaitForSeconds(0.1f);
            renderer.material = original;
            yield return new WaitForSeconds((-0.15f*i)+1.5f);
        }
        particleSystem.Play();
        renderer.enabled = false;
        CmdRadialDamage(ringRadius, damageAmount);        
        yield return new WaitForSeconds(1.0f);
        gameObject.SetActive(false);
        particleSystem.Stop();        
        renderer.enabled = true;
        transform.parent = parentTransform;
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