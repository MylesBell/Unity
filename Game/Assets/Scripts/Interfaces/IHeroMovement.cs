using UnityEngine.EventSystems;

public interface IHeroMovement : IEventSystemHandler {
	void PlayerChangeProgressDirection (ProgressDirection progressDirection);
	void PlayerMoveChannel(MoveDirection moveDirection);
}