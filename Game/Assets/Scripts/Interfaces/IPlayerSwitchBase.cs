using UnityEngine.EventSystems;

public interface IPlayerSwitchBase : IEventSystemHandler {
	// returns a Hero object
	void PlayerSwitchBase(string playerID);
}