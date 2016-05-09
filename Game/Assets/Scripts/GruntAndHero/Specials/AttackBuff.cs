using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AttackBuff : Special
{   
    public float attackIncrease = 10.0f;
    public float attackIncreaseRadius = 10.0f;
    public float attackIncreaseTime = 10.0f;
    private float originalAttack;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,height,0);
    }

    override public void ResetSpecial()
    {
        attackIncrease = 10.0f;
    }

    override public void UpgradeSpecial()
    {
        attackIncrease += 5.0f;
    }
    
    override public void UseSpecial()
    {
        CmdRadialIncreaseAttack(attackIncreaseRadius);
    }
    
    override public void Kill(){
        RpcKill();
    }
    
    [ClientRpc]
    private void RpcKill() {
        gameObject.SetActive(false);
    }

    
    [Command]
    private void CmdRadialIncreaseAttack(float radius) {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider collider in hitColliders) {
            Debug.Log(collider.tag);
            if (CheckColliderIsOnOwnTeam(collider)){
                AllPlays allPlays = collider.gameObject.GetComponent<AllPlays>();
                allPlays.AttackIncrease(new float[2]{attackIncrease, attackIncreaseTime});
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