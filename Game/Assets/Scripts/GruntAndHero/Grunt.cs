using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour {

	private TargetSelect targetSelect;
    [SyncVar] private bool active = false;

    void Start() {
        gameObject.SetActive(active);
    }
    
    public void InitialiseGrunt(TeamID teamIDInput, Vector3 desiredPosition, float channelOffset) {
        if (isServer) {
            active = true;
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (teamIDInput, desiredPosition, channelOffset);
            gameObject.SetActive(active);
            CmdSetActiveState(active);
        }
	}

    [Command]
    public void CmdSetActiveState(bool active) {
        RpcSetActive(active);
    }

    [ClientRpc]
    public void RpcSetActive(bool active) {
        gameObject.SetActive(active);
    }
}