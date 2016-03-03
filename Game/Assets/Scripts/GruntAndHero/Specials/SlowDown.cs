using UnityEngine;
using UnityEngine.Networking;

public class SlowDown : Special
{   
    public float slowDownTime = 10.0f;
    public float slowDownRadius = 10.0f;
    public float slowDownMultiplier = 0.5f;
    
    override public void InitialiseSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void ResetSpecial()
    {
        slowDownMultiplier = 10.0f;
    }

    override public void UpgradeSpecial()
    {
        slowDownMultiplier -= 0.1f;
        if (slowDownMultiplier < 0) slowDownMultiplier = 0;
    }

    override public void UseSpecial()
    {
        CmdRadialSlowDown(slowDownRadius);
    }
    
    // to slow down in circular area about player
    [Command]
    private void CmdRadialSlowDown(float radius){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            if (CheckColliderWantsToAttack(collider)){
                collider.gameObject.GetComponent<UnitSlowDown>().SlowDown(slowDownTime, slowDownMultiplier);
            }
        }
    }
    
    private bool CheckColliderWantsToAttack(Collider collider){
        // check gameobejct exists first (aka not default)
        if (!collider.gameObject.Equals(default(GameObject))) {
            return collider.gameObject.tag.Equals(specials.attackGruntTag) || collider.gameObject.tag.Equals(specials.attackHeroTag);
        }
        
        return false;
    }
}