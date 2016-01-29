using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public enum SpecialType { fire, heal, water };

public class Special : NetworkBehaviour, IPlayerSpecial {
    
    public GameObject FireRingPrefab;
    public GameObject HealRingPrefab;
    
    private GameObject healRingSystem;
    
    private Stats stats;
    
    private string attackGruntTag;
    private string attackHeroTag;
    private string attackBaseTag;

    private string ownGruntTag;
    private string ownHeroTag;
    private string ownBaseTag;

    private Vector3 originalScale = new Vector3(1,1,1);
    private Vector3 currentScale = new Vector3(1,1,1);
    
    void Start() {
        healRingSystem = (GameObject) Instantiate(HealRingPrefab, gameObject.transform.position, HealRingPrefab.transform.rotation);
        healRingSystem.SetActive(false);
        healRingSystem.transform.parent = gameObject.transform;
        
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
        RpcPlayFireParticleSystem(currentScale);
        CmdRadialDamage(stats.fireAttackRadius, stats.fireAttackDamage);
    }

    [ClientRpc]
    public void RpcPlayFireParticleSystem(Vector3 currentScale) {
        GameObject fireRingParticle = (GameObject) Instantiate(FireRingPrefab, gameObject.transform.position, FireRingPrefab.transform.rotation);
        fireRingParticle.transform.localScale = currentScale;
        Destroy(fireRingParticle, fireRingParticle.GetComponent<ParticleSystem>().startLifetime);
    }
    
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
    
    private void HealRing(){
        RpcPlayHealRingSystem();
        CmdRadialHeal(stats.healRingRadius, stats.healRingHealth);
    }

    [ClientRpc]
    public void RpcPlayHealRingSystem() {
        StartCoroutine(PlayHealRingSystem());
    }
    
    private IEnumerator PlayHealRingSystem(){
        healRingSystem.SetActive(true);
        healRingSystem.transform.rotation = HealRingPrefab.transform.rotation;
        yield return new WaitForSeconds(2.0f);
        healRingSystem.SetActive(false);
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
    
    public void UpgradeSpecials(){
        UpgradeFireAttack();
        UpgradeHealthRing();
    }
    
    // to increase size of fire particle attack the scale is increased, then the damage area is incremented
    private void UpgradeFireAttack(){
        currentScale += new Vector3(1.0f, 1.0f, 0);
        stats.fireAttackRadius += 3.0f;
    }
    
    private void UpgradeHealthRing(){
        healRingSystem.transform.localScale += new Vector3(1.0f, 1.0f, 0);
        stats.healRingRadius += 3.0f;
    }
    
    public void ResetSpecials(){
        currentScale = originalScale;
        if(stats){
            stats.resetFireAttackRadius();
            stats.resetHealRingRadius();
        }
    }
}