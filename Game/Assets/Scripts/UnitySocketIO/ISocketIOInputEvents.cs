public interface ISocketIOInputEvents{
	void PlayerJoin (string playerID, string playerName, string gameCode);
	void PlayerChangeProgressDirection (string playerID, ProgressDirection progressDirection);
	void PlayerMoveChannel (string playerID, MoveDirection moveDirection);
    void PlayerUseSpecial(string playerID, SpecialType specialType);
	void PlayerSwitchBase (string playerID);
}