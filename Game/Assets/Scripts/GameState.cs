using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour {
    public enum State { IDLE, PLAYING, END };

    public static State gameState;

    // Use this for initialization
    void Start () {
        gameState = State.IDLE;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.S)) {
            gameState = gameState == State.IDLE ? State.PLAYING: gameState;
        }

        if (Input.GetKeyUp(KeyCode.E)) {
            gameState = gameState == State.PLAYING ? State.END : gameState;
        }

        if (Input.GetKeyUp(KeyCode.Q)) {
            gameState = gameState == State.END ? State.IDLE : gameState;
        }
    }

    void onGameStateChange() {
        //tell sockets io
        switch (gameState) {
            case State.IDLE:

                break;
            case State.PLAYING:

                break;
            case State.END:

                break;
        }
    }

    public static void changeGameState(State state) {
        gameState = state;
    }

    public static void endGame(TeamID winner) {
        changeGameState(State.END);
        Debug.Log(winner + " won!\n");
    }
}
