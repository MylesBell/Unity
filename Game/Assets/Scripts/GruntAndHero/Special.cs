using UnityEngine;
using UnityEngine.Networking;

public class Special : NetworkBehaviour{
    public enum SpecialType { fire, water, air };
    
    public GameObject FireRingParticle;
    
    void Update() {
        
        if (Input.GetKeyUp(KeyCode.X)) {
            EmitSpecial(SpecialType.fire);
        }
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
        GameObject fireRingParticle = (GameObject) Instantiate(FireRingParticle, gameObject.transform.position, FireRingParticle.transform.rotation);
        Destroy(fireRingParticle, fireRingParticle.GetComponent<ParticleSystem>().startLifetime);
        radialDamage();
    }
    
    private void radialDamage(){
        float damageRadius = 7.0f;
        float damage = 30.0f;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach(Collider collider in hitColliders) {
            if (checkColliderWantsToBeHit(collider)){
                ((Health)collider.gameObject.GetComponent ("Health")).reduceHealth(damage);
            }
        }
    }
    
    private bool checkColliderWantsToBeHit(Collider collider){
        
        if (collider.gameObject.tag.Contains("TeamBase") || collider.gameObject.tag.Contains("Grunt")
            || collider.gameObject.tag.Contains("Hero") && !collider.gameObject.tag.Equals(tag)){
            return true;
        }
        return false;
    }
}