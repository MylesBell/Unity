using UnityEngine;
using UnityEngine.Networking;

public abstract class Special : NetworkBehaviour {    
    public Specials specials;
    public Stats stats;
    
    public Vector3 currentScale;
    
    void Start(){
        specials = gameObject.GetComponentInParent<Specials>();
        stats = gameObject.GetComponentInParent<Stats>();
        
        gameObject.SetActive(false);
    }
    
	public abstract void InitialiseSpecial(float height);
    public abstract void UseSpecial();
    public abstract void UpgradeSpecial();
    public abstract void ResetSpecial();
}