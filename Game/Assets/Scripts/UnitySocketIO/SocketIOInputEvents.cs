using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SocketIOInputEvents : ISocketIOInputEvents{

	private GameObject teamsObject;
	private Teams teams;

	public SocketIOInputEvents(){
		teamsObject = GameObject.FindGameObjectWithTag ("GameController");
		teams = teamsObject.GetComponent<Teams> ();
	}

	#region ISocketIOInputEvents implementation
	
	public void PlayerJoin (int playerID)
	{
		ExecuteEvents.Execute<IPlayerJoin> (teamsObject, null, (x,y) => x.PlayerJoin(playerID));
	}

	public void PlayerBack (int playerID)
	{
		throw new System.NotImplementedException ();
	}

	public void PlayerMoveLane (Direction direction)
	{
		throw new System.NotImplementedException ();
	}

	#endregion


}

