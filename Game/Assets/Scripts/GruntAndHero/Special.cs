using UnityEngine;
using UnityEngine.Networking;

public enum SpecialType { fire, water, air };

public class Special : NetworkBehaviour, IPlayerSpecial {
    
    public GameObject FireRingParticle;
    
    private Stats stats;
    private string attackGruntTag;
    private string attackHeroTag;
    private string attackBaseTag;
    
    void Start() {
        stats = gameObject.GetComponent<Stats>();
        
        TargetSelect targetSelect = gameObject.GetComponent<TargetSelect>();
        attackGruntTag = targetSelect.attackGruntTag;
        attackHeroTag = targetSelect.attackHeroTag;
        attackBaseTag = targetSelect.attackBaseTag;
    }
    
    void Update() {
        if (Input.GetKeyUp(KeyCode.X)) {
            EmitSpecial(SpecialType.fire);
        }
    }
    
    // implement IPlayerSpecial interface
    public void PlayerSpecial(SpecialType specialType)
    {
        EmitSpecial(specialType);
    }
    
    public void EmitSpecial(SpecialType specialType){
        switch(specialType){
            case SpecialType.fire:
                FireRing();
                break;
            case SpecialType.water:
                FireRing();
                break;
            default:
                FireRing();
                break; 
        }
    }
    
    private void FireRing(){
        RpcPlayFireParticleSystem();
        CmdRadialDamage(stats.fireAttackRadius, stats.fireAttackDamage);
    }

    [ClientRpc]
    public void RpcPlayFireParticleSystem() {
        GameObject fireRingParticle = (GameObject) Instantiate(FireRingParticle, gameObject.transform.position, FireRingParticle.transform.rotation);
        Destroy(fireRingParticle, fireRingParticle.GetComponent<ParticleSystem>().startLifetime);
    }
    
    [Command]
    private void CmdRadialDamage(float radius, float damage){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            if (CheckColliderWantsToBeHit(collider)){
                ((Health)collider.gameObject.GetComponent<Health>()).reduceHealth(damage);
            }
        }
    }
    
    private bool CheckColliderWantsToBeHit(Collider collider){
        
        if (collider.gameObject.tag.Equals(attackGruntTag) || collider.gameObject.tag.Equals(attackHeroTag)
            || collider.gameObject.tag.Equals(attackBaseTag)){
            return true;
        }
        return false;
    }
    
    // to increase size of fire particle attack the scale is increased, then the damage area is incremented
    void upgradeFireAttack(Stats stats){
        FireRingParticle.GetComponent<ParticleSystem>().transform.localScale += new Vector3(1.0f, 1.0f, 0);
        stats.fireAttackRadius += 3.0f;
    }
}