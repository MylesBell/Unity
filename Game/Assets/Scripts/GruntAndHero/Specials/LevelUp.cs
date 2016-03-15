using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LevelUp : Special
{   
    override public void InitialiseSpecial(float height)
    {   
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,height,0);
    }

    override public void ResetSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
    }

    override public void UpgradeSpecial()
    {
        currentScale += new Vector3(1.0f, 1.0f, 0);
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlayLevelUp();
    }

    [ClientRpc]
    private void RpcPlayLevelUp() {
        gameObject.SetActive(true);
        StartCoroutine(PlayLevelUp());
    }
    
    private IEnumerator PlayLevelUp(){
        yield return new WaitForSeconds(2.0f);
        gameObject.SetActive(false);
    }
}