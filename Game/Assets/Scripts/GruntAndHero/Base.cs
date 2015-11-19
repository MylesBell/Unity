using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Base : NetworkBehaviour {
    private TeamID teamID;

    public void initialise(TeamID team) {
        //set Health to Max
        gameObject.GetComponent<Health>().initialiseHealth();
        teamID = team;
    }
    void OnDestroy() {
        //return the winner
        if(GameState.gameState == GameState.State.PLAYING) GameState.endGame(teamID == TeamID.blue ? TeamID.red : TeamID.blue);
    }
}
