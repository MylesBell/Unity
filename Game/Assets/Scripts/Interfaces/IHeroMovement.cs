using UnityEngine.EventSystems;

public interface IHeroMovement : IEventSystemHandler {
	void PlayerBack ();
	void PlayerMoveChannel(MoveDirection moveDirection);
}
