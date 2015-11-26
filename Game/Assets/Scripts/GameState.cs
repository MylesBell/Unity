using UnityEngine;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {
    public enum State { IDLE, PLAYING, END };

    public static State gameState;
    public static TeamID winningTeam;

    // Use this for initialization
    void Start () {
        if(isServer) gameState = State.IDLE;
    }
	
	// Update is called once per frame
	void Update () {
        if (isServer) { 
            if (Input.GetKeyUp(KeyCode.S)) {
                gameState = gameState == State.IDLE ? State.PLAYING: gameState;
			    startGame ();
            }

            if (Input.GetKeyUp(KeyCode.E)) {
                gameState = gameState == State.PLAYING ? State.END : gameState;
            }

            if (Input.GetKeyUp(KeyCode.Q)) {
                gameState = gameState == State.END ? State.IDLE : gameState;
            }
            string text;
            if (gameState == State.IDLE) {
                text = "Preparing...";
            } else if (gameState == State.END) {
                text = "Team " + winningTeam.ToString().ToUpper() + " won!";
            } else {
                text = "";
            }
            CmdSetText(text);
        }
    }

    public static void changeGameState(State state) {
        gameState = state;
		SocketIOOutgoingEvents.GameStateChange (state);
    }

	public static void endGame(TeamID winner) {
		changeGameState(State.END);
		winningTeam = winner;

		Debug.Log(winner + " won!\n");
	}

	public static void startGame() {
		changeGameState(State.PLAYING);
	}

    [Command]
    public void CmdSetText(string text) {
        RpcSetText(text);
        SetText(text);
    }

    [ClientRpc]
    public void RpcSetText(string text) {
        SetText(text);
    }

    public void SetText(string text) {
        Camera.main.transform.FindChild("GameStateText").GetComponent<TextMesh>().text = text;

    }
}
