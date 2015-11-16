using UnityEngine.EventSystems;

public interface IPlayerJoin : IEventSystemHandler {
	// returns a Hero object
	void PlayerJoin(int playerID, string playerName);
}