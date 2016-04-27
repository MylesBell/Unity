using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DefenseBuff : Special
{   
    public float defenceIncrease = 0.5f;
    public float defenceIncreaseRadius = 10.0f;
    public float defenceIncreaseTime = 10.0f;
    private float originalDefence;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,height,0);
    }

    override public void ResetSpecial()
    {
        defenceIncrease = 0.5f;
    }

    override public void UpgradeSpecial()
    {
        defenceIncrease += 0.5f;
    }
    
    override public void UseSpecial()
    {
        CmdRadialIncreaseDefence(defenceIncreaseRadius);
    }

    [Command]
    private void CmdRadialIncreaseDefence(float radius) {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            Debug.Log(collider.tag);
            if (CheckColliderIsOnOwnTeam(collider)){
                AllPlays allPlays = collider.gameObject.GetComponent<AllPlays>();
                allPlays.DefenceIncrease(new float[2]{defenceIncrease, defenceIncreaseTime});
            }
        }
    }
    
    private bool CheckColliderIsOnOwnTeam(Collider collider){
        // check gameobejct exists first (aka not default)
        if (!collider.gameObject.Equals(default(GameObject))) {
            return collider.gameObject.tag.Equals(specials.ownGruntTag) || collider.gameObject.tag.Equals(specials.ownHeroTag);
        }
        
        return false;
    }
}