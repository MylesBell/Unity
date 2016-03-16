using UnityEngine;
using UnityEngine.Networking;

public class AllPlays : NetworkBehaviour {
    
    // prefabs
    public GameObject unitSlowDownPrefab;
    
    // all play objects 
    private AllPlay slowDown;
    
    void Start() {

    }
    
    public void InitialiseAllPlays(){
        // init all allPlay objects
        slowDown = createAllPlay(unitSlowDownPrefab);
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
}