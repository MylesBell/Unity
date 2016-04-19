using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

public class DamageText : NetworkBehaviour {
    public GameObject damageTextPrefab;
    private int damageTextPoolSize = 10;
    private LinkedList<GameObject> availableDamageTexts = new LinkedList<GameObject>();
    private LinkedList<GameObject> inUseDamageTexts = new LinkedList<GameObject>();
    
    private ComputerLane computerLane;
    
	public void InitialiseDamageText(ComputerLane computerLane){
        // when pool not initialised
        if (availableDamageTexts.Count == 0 && inUseDamageTexts.Count == 0){
		    InitialiseDamageTextPool(computerLane);    
        }else{
            ResetDamageTextPool();
        }
	}
    
    public void SetComputerLane(ComputerLane computerLane){
        RpcSetComputerLane(computerLane);
    }
    
    [ClientRpc]
    private void RpcSetComputerLane(ComputerLane computerLane){
        this.computerLane = computerLane;
    }
	
	public void Play(float damage){
		GameObject damageTextObject = GetDamageText();
        damageTextObject.SetActive(true);
        RpcPlay(damage, damageTextObject);
    }
    
    [ClientRpc]
    public void RpcPlay(float damage, GameObject damageTextObject) {
        damageTextObject.SetActive(true);
        
        // set colour for increase or decrease
        if (damage > 0){
            damageTextObject.GetComponent<TextMesh>().color = Color.green;
        }else{
            damageTextObject.GetComponent<TextMesh>().color = Color.red;
        }
        
        damageTextObject.GetComponent<TextMesh>().text = Mathf.Abs(damage).ToString();
        damageTextObject.GetComponent<Animator>().SetTrigger("Damage");
        if (gameObject.activeSelf == true) StartCoroutine(ReturnDamageTextToPool(damageTextObject));
    }
    
    IEnumerator ReturnDamageTextToPool(GameObject damageTextObject) {
        yield return new WaitForSeconds(1.2f);
        damageTextObject.SetActive(false);
        if (isServer){
            lock (availableDamageTexts) {
                inUseDamageTexts.RemoveFirst();
                availableDamageTexts.AddLast(damageTextObject);
            }
        }
    }
    
	// fix the rotation
    void Update(){
        foreach (GameObject damageTextObject in inUseDamageTexts){
            RpcSetRotation(damageTextObject);
        }
    }
    
    [ClientRpc]
    private void RpcSetRotation(GameObject damageTextObject){
        damageTextObject.transform.rotation = (computerLane == ComputerLane.RIGHT)? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
    }
    
    public void InitialiseDamageTextPool(ComputerLane computerLane) {
        availableDamageTexts = new LinkedList<GameObject>();
        inUseDamageTexts = new LinkedList<GameObject>();
        
        this.computerLane = computerLane;
        RpcSetComputerLane(computerLane);
        
        for(int i = 0; i < damageTextPoolSize; i++) {
            GameObject damageText = InitDamageText();
            damageText.SetActive(false);
            availableDamageTexts.AddLast(damageText);
        }
    }
    
    
    private void ResetDamageTextPool(){
        // clear not used pool and set inactive
        lock (availableDamageTexts) {
            while(inUseDamageTexts.Count > 0){
                GameObject damageTextObject = inUseDamageTexts.First.Value;
                damageTextObject.SetActive(false);
                inUseDamageTexts.RemoveFirst();
                availableDamageTexts.AddLast(damageTextObject);
            }
        }
    }
    
    private GameObject InitDamageText(){   
        GameObject damageTextObject = (GameObject) Instantiate(damageTextPrefab, gameObject.transform.position,
            damageTextPrefab.transform.rotation);
        NetworkServer.Spawn(damageTextObject);
        
        RpcSetParent(damageTextObject, gameObject);        
        damageTextObject.transform.parent = gameObject.transform;
        float height = gameObject.GetComponent<BoxCollider>().size.y / 2;
        damageTextObject.transform.localPosition = new Vector3(0, height, 0);
        
        return damageTextObject;
    }
    
	private GameObject GetDamageText() {
        GameObject damageText;
        lock (availableDamageTexts) { 
            if (availableDamageTexts.Count > 0) {
                damageText = availableDamageTexts.First.Value;
                availableDamageTexts.RemoveFirst();
                inUseDamageTexts.AddLast(damageText);
            } else {
                damageText = InitDamageText();
            }
        }
        return damageText;
    }
    
    [ClientRpc]
    private void RpcSetParent(GameObject child, GameObject parent) {
        child.transform.parent = parent.transform;
    }
	
}
