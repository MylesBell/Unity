using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Invisibility : Special
{   
    public GameObject originalModel;
    public GameObject decoyModel;
    private float invisibilityTime = 4.0f;
    
    private Team enemyTeam = null;
    private Teams teams;
    private float originalSpeed;
    
    override public void InitialiseSpecial(float height)
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        transform.localPosition = new Vector3(0,0,0);
        RpcFindModels();
        teams = GameObject.FindGameObjectWithTag("GameController").GetComponent<Teams>();
    }

    override public void ResetSpecial()
    {
        invisibilityTime = 4.0f;
    }

    override public void UpgradeSpecial()
    {
        invisibilityTime += 2.0f;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        originalModel.SetActive(false);
        decoyModel.SetActive(true);
        originalSpeed = stats.movementSpeed;
        stats.movementSpeed = 10;
        RpcPlayInvisibiltySystem();
        CmdSetNotAttackable();
    }
    
    override public void Kill(){
        RpcKill();
    }
    
    [ClientRpc]
    private void RpcKill() {
        StopAllCoroutines();
        originalModel.SetActive(true);
        decoyModel.SetActive(false);
        gameObject.SetActive(false);
        transform.parent.gameObject.GetComponent<HeroMovement>().SwitchAnimator(originalModel.GetComponent<Animator>());
        transform.parent.gameObject.GetComponent<Attack>().SwitchAnimator(originalModel.GetComponent<Animator>());
        transform.parent.gameObject.GetComponent<Hero>().SwitchAnimator(originalModel.GetComponent<Animator>());
        if (isServer){
            SetAttackable();
            stats.movementSpeed = originalSpeed;
        }
    }
    
    [ClientRpc]
    public void RpcFindModels() {
        foreach (Transform child in transform.parent) {
            if (child.tag == "originalModel")
                originalModel = child.gameObject;
            if (child.tag == "decoyModel")
                decoyModel = child.gameObject;
        }
    }
    
    [ClientRpc]
    public void RpcPlayInvisibiltySystem() {
        gameObject.SetActive(true);
        originalModel.SetActive(false);
        decoyModel.SetActive(true);
        transform.parent.gameObject.GetComponent<HeroMovement>().SwitchAnimator(decoyModel.GetComponent<Animator>());
        transform.parent.gameObject.GetComponent<Attack>().SwitchAnimator(decoyModel.GetComponent<Animator>());
        transform.parent.gameObject.GetComponent<Hero>().SwitchAnimator(decoyModel.GetComponent<Animator>());
        StartCoroutine(PlayInvisibiltySystem());
    }
    
    IEnumerator PlayInvisibiltySystem(){
        yield return new WaitForSeconds(invisibilityTime);
        gameObject.SetActive(false);
        originalModel.SetActive(true);
        decoyModel.SetActive(false);
        transform.parent.gameObject.GetComponent<HeroMovement>().SwitchAnimator(originalModel.GetComponent<Animator>());
        transform.parent.gameObject.GetComponent<Attack>().SwitchAnimator(originalModel.GetComponent<Animator>());
        transform.parent.gameObject.GetComponent<Hero>().SwitchAnimator(originalModel.GetComponent<Animator>());
        if (isServer){
            SetAttackable();
            stats.movementSpeed = originalSpeed;
        }
    }
    
    [Command]
    private void CmdSetNotAttackable(){
        // first time get enemy team, not done in init as specials null then.
        if (enemyTeam == null){
            setEnemyTeam();
        }
        enemyTeam.setNotAttackable(specials.gameObject);
    }
    
    private void SetAttackable(){
        // first time get enemy team, not done in init as specials null then.
        if (enemyTeam == null){
            setEnemyTeam();
        }
        enemyTeam.setAttackable(specials.gameObject);
    }
    
    private void setEnemyTeam(){
        if (teams.blueTeam.teamID == specials.teamID){
            enemyTeam = teams.redTeam; 
        }else{
            enemyTeam = teams.blueTeam; 
        }
    }
}