using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UnitSlowDown : NetworkBehaviour {
    
    public GameObject SlowDownPrefab;
    private GameObject slowDownParticle;
    private float savedSpeed;
    
    void Start() {
        InitialiseSlowDown();
    }
    
    public void InitialiseSlowDown(){
        // initialise slow down
        slowDownParticle = (GameObject) Instantiate(SlowDownPrefab, gameObject.transform.position,
                SlowDownPrefab.transform.rotation);
        NetworkServer.Spawn(slowDownParticle);
        RpcSetParent(slowDownParticle,gameObject);
        RpcSetRotation(slowDownParticle, SlowDownPrefab.transform.rotation);
        slowDownParticle.transform.parent = gameObject.transform;
        slowDownParticle.SetActive(false);
    }
    
    [ClientRpc]
    public void RpcSetParent(GameObject child, GameObject parent) {
        child.transform.parent = parent.transform;
    }
    
    [ClientRpc]
    public void RpcSetRotation(GameObject targetObject, Quaternion rotation) {
        targetObject.transform.rotation = rotation;
    }
    
    public void SlowDown(float slowDownTime, float slowDownMultiplier){
        CmdReduceSpeed(slowDownMultiplier);
        RpcPlaySlowDown(slowDownTime);
    }
    
    [Command]
    private void CmdReduceSpeed(float slowDownMultiplier){
        savedSpeed = gameObject.GetComponent<Stats>().movementSpeed;
        gameObject.GetComponent<Stats>().movementSpeed = savedSpeed * slowDownMultiplier;
    }
    
    [Command]
    private void CmdResumeSpeed(){
        gameObject.GetComponent<Stats>().movementSpeed = savedSpeed;
    }
    
    [ClientRpc]
    public void RpcPlaySlowDown(float slowDownTime) {
        slowDownParticle.SetActive(true);
        StartCoroutine(PlaySlowDown(slowDownTime));
    }
    
    IEnumerator PlaySlowDown(float slowDownTime){
        yield return new WaitForSeconds(slowDownTime);
        slowDownParticle.SetActive(false);
        CmdResumeSpeed(); // cmd from within rpc #dubious
    }
}