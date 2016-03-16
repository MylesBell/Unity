using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SpeedBuff : Special
{   
    public float speedIncrease = 5.0f;
    private float originalSpeed;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,height,0);
    }

    override public void ResetSpecial()
    {
        speedIncrease = 5.0f;
    }

    override public void UpgradeSpecial()
    {
        speedIncrease += 1.0f;
    }

    override public void UseSpecial()
    {
        originalSpeed = stats.movementSpeed;
        gameObject.SetActive(true);
        RpcPlaySpeedBuffSystem();
    }
    
    [ClientRpc]
    public void RpcPlaySpeedBuffSystem() {
        stats.movementSpeed += speedIncrease;
        gameObject.SetActive(true);
        StartCoroutine(PlaySpeedBuffSystem());
    }
    
    IEnumerator PlaySpeedBuffSystem(){
        yield return new WaitForSeconds(5.0f);
        gameObject.SetActive(false);
        stats.movementSpeed = originalSpeed;
    }
}