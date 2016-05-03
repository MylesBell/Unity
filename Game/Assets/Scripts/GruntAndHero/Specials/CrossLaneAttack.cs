using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CrossLaneAttack : Special
{   
    public float beamRadius = 4.0f;
    public float damageAmount = 80.0f;
    private Transform parentTransform;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        RpcSetParentTransform();
    }

    override public void ResetSpecial()
    {
        beamRadius = 4.0f;
        damageAmount = 80.0f;
        gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void UpgradeSpecial()
    {
        // to increase size of fire particle attack the scale is increased, then the damage area is incremented
        currentScale += new Vector3(1.0f, 1.0f, 0);
        beamRadius += 1.0f;
    }

    override public void UseSpecial()
    {
        transform.parent = null;
        transform.position = new Vector3(parentTransform.position.x,
                                         parentTransform.position.y,
                                         150);
        transform.rotation = Quaternion.Euler(-90,90,0);                                         
        gameObject.SetActive(true);
        RpcPlayCrossLaneAttackSystem();
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
    public void RpcPlayCrossLaneAttackSystem() {
        transform.parent = null;        
        transform.position = new Vector3(parentTransform.position.x,
                                         parentTransform.position.y,
                                         150);   
        transform.rotation = Quaternion.Euler(-90,90,0);    
        gameObject.SetActive(true);
        StartCoroutine(PlayCrossLaneAttackSystem());
    }
    
    [ClientRpc]
    public void RpcSetParentTransform() {
        parentTransform = transform.parent;
    }
    
    IEnumerator PlayCrossLaneAttackSystem() {
        ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
        particleSystem.Play();        
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
        particleSystem.Stop();
        transform.parent = parentTransform;
    }
    
    void OnTriggerEnter(Collider collider) {
        if (isServer) {
            if (CheckColliderWantsToAttack(collider)) {
                bool killedObject;
                ((Health)collider.gameObject.GetComponent<Health>()).ReduceHealth(damageAmount, out killedObject);
                if(killedObject){
                    stats.IncrementKills(collider.gameObject.GetComponent<Hero>() != null);
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