using UnityEngine;
using UnityEngine.EventSystems;

public class SocketIOInputEvents : ISocketIOInputEvents{

	private GameObject teamsObject;
	private Teams teams;

	public SocketIOInputEvents(){
		teamsObject = GameObject.FindGameObjectWithTag ("GameController");
		teams = teamsObject.GetComponent<Teams> ();
	}

	#region ISocketIOInputEvents implementation
	
	public void PlayerJoin (string playerID, string playerName, string gameCode)
	{
		ExecuteEvents.Execute<IPlayerJoin> (teamsObject, null, (x,y) => x.PlayerJoin(playerID, playerName, gameCode));
	}

	public void PlayerChangeProgressDirection (string playerID, ProgressDirection progressDirection)
	{
		ExecuteEvents.Execute<IHeroMovement> (teams.GetHero(playerID), null, (x,y) => x.PlayerChangeProgressDirection(progressDirection));
	}

	public void PlayerMoveChannel (string playerID, MoveDirection moveDirection)
	{
		ExecuteEvents.Execute<IHeroMovement> (teams.GetHero(playerID), null, (x,y) => x.PlayerMoveChannel(moveDirection));
	}

	#endregion


}

