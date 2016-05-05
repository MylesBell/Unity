using UnityEngine;
using UnityEngine.Networking;

public abstract class AllPlay : NetworkBehaviour {
    [SyncVar]
    public NetworkInstanceId parentNetId;
     
    public Vector3 currentScale;
    public Stats stats;
    
    void Start(){
        stats = gameObject.GetComponentInParent<Stats>();
        gameObject.SetActive(false);
    }
    
    public override void OnStartClient()
    {
        // When we are spawned on the client,
        // find the parent object using its ID,
        // and set it to be our transform's parent.
        GameObject parentObject = ClientScene.FindLocalObject(parentNetId);
        transform.SetParent(parentObject.transform);
    }
    
    public Stats GetStats(){
        if (stats == null){
            stats = gameObject.GetComponentInParent<Stats>();
        }
        return stats;
    }
    
	public abstract void Initialise(float height);
    public abstract void Use(params float[] inputs);
    public abstract void Upgrade();
    public abstract void Reset();
    
    public abstract void Kill();
}