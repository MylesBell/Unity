using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Invisibility : Special
{   
    public float invisibilityTime = 10.0f;
    
    private Team enemyTeam = null;
    private Teams teams;
    
    override public void InitialiseSpecial()
    {
        currentScale = new Vector3(1.0f, 1.0f, 0);
        
        teams = GameObject.FindGameObjectWithTag("GameController").GetComponent<Teams>();
    }

    override public void ResetSpecial()
    {
        invisibilityTime = 10.0f;
    }

    override public void UpgradeSpecial()
    {
        invisibilityTime += 5.0f;
    }

    override public void UseSpecial()
    {
        gameObject.SetActive(true);
        RpcPlayInvisibiltySystem();
        CmdSetNotAttackable();
    }
    
    [ClientRpc]
    public void RpcPlayInvisibiltySystem() {
        gameObject.SetActive(true);
        StartCoroutine(PlayInvisibiltySystem());
    }
    
    IEnumerator PlayInvisibiltySystem(){
        yield return new WaitForSeconds(invisibilityTime);
        gameObject.SetActive(false);
        if (isServer){
            SetAttackable();
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