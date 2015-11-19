// SocketIO methods to handle game events;
using UnityEngine;

public enum State { IDLE, PLAYING, END };

public static class SocketIOOutgoingEvents {
	#region ISocketIOOutgoingEvents implementation

	public static void GameStateChange (GameState.State state)
	{
		SocketNetworkManager socketIOManager = new SocketNetworkManager ();
		socketIOManager.GameStateHandler (state);
	}

	public static void HeroDeath ()
	{
		throw new System.NotImplementedException ();
	}

	#endregion
}