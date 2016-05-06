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
	
    // join and leave
	public void PlayerJoin (string playerID, string playerName, int playerClass, string gameCode)
	{
		ExecuteEvents.Execute<IPlayerJoin> (teamsObject, null, (x,y) => x.PlayerJoin(playerID, playerName, playerClass, gameCode));
	}
    
    public void PlayerLeave (string playerID)
    {
        ExecuteEvents.Execute<IPlayerLeave> (teamsObject, null, (x,y) => x.PlayerLeave(playerID));
    }
       
    // movement
	public void PlayerMovement (string playerID, MoveDirection moveDirection)
	{
		ExecuteEvents.Execute<IHeroMovement> (teams.GetHero(playerID), null, (x,y) => x.PlayerMovement(moveDirection));
	}

    // special
    public void PlayerUseSpecial(string playerID, SpecialType specialType)
    {
        ExecuteEvents.Execute<IPlayerSpecial> (teams.GetHero(playerID), null, (x,y) => x.PlayerSpecial(specialType));
    }

	#endregion


}

