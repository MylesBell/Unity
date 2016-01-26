using UnityEngine.EventSystems;

public interface IPlayerLeave : IEventSystemHandler {
	// returns a Hero object
	void PlayerLeave(string playerID);
}