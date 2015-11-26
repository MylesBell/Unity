public interface ISocketIOInputEvents{
	void PlayerJoin (string playerID, string playerName);
	void PlayerChangeProgressDirection (string playerID, ProgressDirection progressDirection);
	void PlayerMoveChannel (string playerID, MoveDirection moveDirection);
}