using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Hero : NetworkBehaviour, IHeroMovement, IDisableGO {
    [SyncVar] public string HeroNameString = "";
    private Team team;
	private TargetSelect targetSelect;
    [SyncVar] private bool active = false;

    public void Start() {
        GameObject heroname = transform.FindChild("HeroName").gameObject;
        heroname.GetComponent<TextMesh>().text = HeroNameString;
        gameObject.SetActive(active);
    }

	public void InitialiseHero(Team team, string playerName, Vector3 spawnLocation, Vector3 desiredPosition, float channelOffset) {
        GameObject heroname = transform.FindChild("HeroName").gameObject;
        HeroNameString = playerName;
        heroname.GetComponent<TextMesh>().text = HeroNameString;
        if (isServer) {
            active = true;
            this.team = team;
            gameObject.GetComponent<Attack>().initiliseAttack();
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (team.GetTeamID(), desiredPosition, channelOffset);
            gameObject.SetActive(active);
            CmdSetActiveState(active);
        }
    }

    public void resetHero(Vector3 spawnLocation, Vector3 desiredPosition, float channelOffset) {
        if (isServer) {
            active = true;
            gameObject.GetComponent<Movement>().initialiseMovement(spawnLocation);
            gameObject.GetComponent<Attack>().initiliseAttack();
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (team.GetTeamID(), desiredPosition, channelOffset);
            gameObject.SetActive(active);
            CmdSetActiveState(active);
        }
    }

    [Command] public void CmdSetActiveState(bool active) {
        RpcSetActive(active);
    }

    [ClientRpc]
    public void RpcSetActive(bool active) {
        gameObject.SetActive(active);
    }

    void onDestroy() {
        //fire event to SocketIo that hero is dead
    }

    public string getHeroName()
    {
        return HeroNameString;
    }

    #region IHeroMovement implementation
    public void PlayerBack ()
	{
		throw new System.NotImplementedException ();
	}
	public void PlayerMoveChannel (MoveDirection moveDirection)
	{
        if (isServer) {
            targetSelect.MoveToZOffset(moveDirection);
        }
    }
    #endregion

    public void disableGameObject() {
        active = false;
        gameObject.SetActive(active);
        CmdSetActiveState(active);
        team.OnHeroDead(gameObject);
    }
}
