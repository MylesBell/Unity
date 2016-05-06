using UnityEngine;
using UnityEngine.Networking;

public class AllPlays : NetworkBehaviour {
    
    // prefabs
    public GameObject unitSlowDownPrefab;
    public GameObject unitAttackIncreasePrefab;
    public GameObject unitDefenceIncreasePrefab;
    public GameObject attackEffectPrefab;
    
    // all play objects 
    private AllPlay slowDown;
    private AllPlay attackIncrease;
    private AllPlay defenceIncrease;
    private AllPlay attackEffect;
    
    
    public void InitialiseAllPlays(){
        // init all allPlay objects
        slowDown = createAllPlay(unitSlowDownPrefab);
        attackIncrease = createAllPlay(unitAttackIncreasePrefab);
        defenceIncrease = createAllPlay(unitDefenceIncreasePrefab);
        attackEffect = createAllPlay(attackEffectPrefab);
    }
    
    private AllPlay createAllPlay(GameObject prefab){

        GameObject allPlayObject = (GameObject) Instantiate(prefab, gameObject.transform.position, prefab.transform.rotation);        
        AllPlay allPlay = allPlayObject.GetComponent<AllPlay>();
        allPlay.parentNetId = gameObject.GetComponent<NetworkIdentity>().netId;
        allPlay.transform.parent = gameObject.transform;
        
        NetworkServer.Spawn(allPlayObject);
        
        float height =  gameObject.GetComponent<BoxCollider>().size.y;             
        allPlay.Initialise(height);
              
        return allPlay;
    }
    
    // methods to use allplays
    public void SlowDown(params float[] input){
        slowDown.Use(input);
    }
    
    public void AttackIncrease(params float[] input){
        attackIncrease.Use(input);
    }
    
    public void DefenceIncrease(params float[] input){
        defenceIncrease.Use(input);
    }
    
    public void AttackEffect(params float[] input){
        attackEffect.Use(input);
    }
    
    public void KillAll() {
        slowDown.Kill();
        attackIncrease.Kill();
        defenceIncrease.Kill();
        attackEffect.Kill();
    }
}