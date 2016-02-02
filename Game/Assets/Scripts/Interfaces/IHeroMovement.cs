using UnityEngine.EventSystems;

public interface IHeroMovement : IEventSystemHandler {
	void PlayerMovement (MoveDirection moveDirection);
}