using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour {
    public enum State { IDLE, PLAYING, END };

    public static State gameState;
	public static TeamID winningTeam;

    // Use this for initialization
    void Start () {
        gameState = State.IDLE;
    }
	
	// Update is called once per frame
	void Update () {
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
    }

    public static void changeGameState(State state) {
        gameState = state;
		SocketIOOutgoingEvents.GameStateChange (state);
    }

	public static void endGame(TeamID winner) {
		changeGameState(State.END);
		GameState.winningTeam = winner;

		Debug.Log(winner + " won!\n");
	}

	public static void startGame() {
		changeGameState(State.PLAYING);
	}
}
