using UnityEngine;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {
    public const string IDLE_STRING = "Preparing...";
    public enum State { IDLE, PLAYING, END };

    public static State gameState;
    public static TeamID winningTeam;
    
    public State networkGameState;
    public static GameState instance;

    // Use this for initialization
    void Start () {
        instance = this;
        gameState = State.IDLE;
        SetText(IDLE_STRING);        
        if(isServer) {
            RpcStateAndText(gameState);
        }
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
            // RpcStateAndText(gameState);
        }
    }

    public static void changeGameState(State state) {
        gameState = state;
        instance.RpcStateAndText(gameState);
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

    [ClientRpc]
    public void RpcStateAndText(GameState.State networkGameState) {
        this.networkGameState = networkGameState;
        string text;
        switch(networkGameState){
            case State.IDLE:
                text = IDLE_STRING;
                break;
            case State.END:
                text = winningTeam.ToString().ToUpper() + " team won!";
                break;
            default:
                text = "";
                break;
        }
        SetText(text);
    }

    public void SetText(string text) {
        Camera.main.transform.FindChild("GameStateText").GetComponent<TextMesh>().text = text;

    }
}
