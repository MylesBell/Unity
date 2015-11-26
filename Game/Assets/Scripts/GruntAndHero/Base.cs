using UnityEngine;
using UnityEngine.Networking;

public class Base : NetworkBehaviour, IDestroyableGameObject {
    private Team team;
    [SyncVar] private bool active = false;

    void Start() {
        gameObject.SetActive(active);
    }
    

    public void InitialiseGameObject(Team team) {
        this.team = team;
    }

    public void ResetGameObject(Vector3 spawnPosition, Vector3 desiredPosition, float channelOffset) {
        if (isServer) {
            active = true;
            gameObject.GetComponent<Health>().initialiseHealth();
            gameObject.SetActive(active);
            gameObject.transform.position = spawnPosition;
            CmdSetActiveState(active, spawnPosition);
        }
    }

    [Command]
    public void CmdSetActiveState(bool active, Vector3 position) {
        RpcSetActive(active, position);
    }

    [ClientRpc]
    public void RpcSetActive(bool active, Vector3 position) {
        gameObject.SetActive(active);
        gameObject.transform.position = position;
    }

    public void DisableGameObject() {
        GameState.endGame(team.GetTeamID() == TeamID.blue ? TeamID.red : TeamID.blue);
        active = false;
        CmdSetActiveState(false, Vector3.zero);
    }
}
