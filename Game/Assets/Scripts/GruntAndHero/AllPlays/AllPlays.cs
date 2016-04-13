using UnityEngine;
using UnityEngine.Networking;

public class AllPlays : NetworkBehaviour {
    
    // prefabs
    public GameObject unitSlowDownPrefab;
    public GameObject attackEffectPrefab;
    
    // all play objects 
    private AllPlay slowDown;
    private AllPlay attackEffect;
    
    void Start() {

    }
    
    public void InitialiseAllPlays(){
        // init all allPlay objects
        slowDown = createAllPlay(unitSlowDownPrefab);
        //attackEffect = createAllPlay(attackEffectPrefab);
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
    
    public void AttackEffect(params float[] input){
        attackEffect.Use(input);
    }
}