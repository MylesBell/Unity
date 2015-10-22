using UnityEngine.EventSystems;

public interface IHeroMovement : IEventSystemHandler {
	void PlayerBack (int playerID);
	void PlayerMoveLane(Direction direction);
}
