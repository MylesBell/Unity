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
	
	public void PlayerJoin (string playerID, string playerName)
	{
		ExecuteEvents.Execute<IPlayerJoin> (teamsObject, null, (x,y) => x.PlayerJoin(playerID, playerName));
	}

	public void PlayerBack (string playerID)
	{
		ExecuteEvents.Execute<IHeroMovement> (teams.GetHero(playerID), null, (x,y) => x.PlayerBack());
	}

	public void PlayerMoveChannel (string playerID, Channel channel)
	{
		Debug.Log(teams.GetHero(playerID));
		ExecuteEvents.Execute<IHeroMovement> (teams.GetHero(playerID), null, (x,y) => x.PlayerMoveChannel(channel));
	}

	#endregion


}

