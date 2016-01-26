using UnityEngine;
using UnityEngine.Networking;

public class Special : NetworkBehaviour{
    public enum SpecialType { fire, water, air };
    
    public GameObject FireRingParticle;
    
    void Update() {
        
        if (Input.GetKeyUp(KeyCode.X)) {
            EmitSpecial(SpecialType.fire);
            Debug.Log("fired fire");
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
    }
}