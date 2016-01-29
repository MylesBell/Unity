using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public enum SpecialType { fire, heal, water };

public class Special : NetworkBehaviour, IPlayerSpecial {
    
    public GameObject FireRingPrefab;
    public GameObject HealRingPrefab;
    
    private GameObject healthRingSystem;
    
    private Stats stats;
    
    private string attackGruntTag;
    private string attackHeroTag;
    private string attackBaseTag;
    private string ownGruntTag;
    private string ownHeroTag;
    private string ownBaseTag;
    
    void Start() {
        healthRingSystem = (GameObject) Instantiate(HealRingPrefab, gameObject.transform.position, HealRingPrefab.transform.rotation);
        healthRingSystem.SetActive(false);
        healthRingSystem.transform.parent = gameObject.transform;
        
        stats = gameObject.GetComponent<Stats>();
        
        TargetSelect targetSelect = gameObject.GetComponent<TargetSelect>();
        attackGruntTag = targetSelect.attackGruntTag;
        attackHeroTag = targetSelect.attackHeroTag;
        attackBaseTag = targetSelect.attackBaseTag;
        ownGruntTag = targetSelect.teamID == TeamID.red ? "redGrunt" : "blueGrunt";
        ownHeroTag = targetSelect.teamID == TeamID.red ? "redHero" : "blueHero";
        ownBaseTag = targetSelect.teamID == TeamID.red ? "redBase" : "blueBase";
    }
    
    void Update() {
        gameObject.transform.rotation = Quaternion.Euler(90,0,0);
        if (Input.GetKeyUp(KeyCode.X)) {
            EmitSpecial(SpecialType.fire);
        }
        if (Input.GetKeyUp(KeyCode.C)) {
            EmitSpecial(SpecialType.heal);
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
            case SpecialType.heal:
                HealRing();
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
        GameObject fireRingAnimation = (GameObject) Instantiate(FireRingPrefab, gameObject.transform.position, FireRingPrefab.transform.rotation);
        Destroy(fireRingAnimation, fireRingAnimation.GetComponent<ParticleSystem>().startLifetime);
    }
    
    [Command]
    private void CmdRadialDamage(float radius, float damage){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            if (CheckColliderWantsToAttack(collider)){
                ((Health)collider.gameObject.GetComponent<Health>()).ReduceHealth(damage);
            }
        }
    }
    
     private void HealRing(){
        RpcPlayHealthRingSystem();
        CmdRadialHeal(stats.fireAttackRadius, stats.fireAttackDamage);
    }

    [ClientRpc]
    public void RpcPlayHealthRingSystem() {
        StartCoroutine(PlayHealthRingSystem());
    }
    
    private IEnumerator PlayHealthRingSystem(){
        healthRingSystem.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        healthRingSystem.SetActive(false);
    }
    
    [Command]
    private void CmdRadialHeal(float radius, float heal){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            if (CheckColliderWantsToHeal(collider)){
                ((Health)collider.gameObject.GetComponent<Health>()).IncreaseHealth(heal);
            }
        }
    }
    
    private bool CheckColliderWantsToAttack(Collider collider){
        
        if (collider.gameObject.tag.Equals(attackGruntTag) || collider.gameObject.tag.Equals(attackHeroTag)
            || collider.gameObject.tag.Equals(attackBaseTag)){
            return true;
        }
        return false;
    }
    
    private bool CheckColliderWantsToHeal(Collider collider){
        
        if (collider.gameObject.tag.Equals(ownGruntTag) || collider.gameObject.tag.Equals(ownHeroTag)
            || collider.gameObject.tag.Equals(ownBaseTag)){
            return true;
        }
        return false;
    }
    
    void upgradeSpecials(){
        upgradeFireAttack();
        upgradeHealthRing();
    }
    
    // to increase size of fire particle attack the scale is increased, then the damage area is incremented
    void upgradeFireAttack(){
        FireRingPrefab.GetComponent<ParticleSystem>().transform.localScale += new Vector3(1.0f, 1.0f, 0);
        stats.fireAttackRadius += 3.0f;
    }
    
    void upgradeHealthRing(){
        healthRingSystem.transform.localScale += new Vector3(1.0f, 1.0f, 0);
        stats.fireAttackRadius += 3.0f;
    }
}