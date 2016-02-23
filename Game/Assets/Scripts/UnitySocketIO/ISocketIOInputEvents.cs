public interface ISocketIOInputEvents{
	void PlayerJoin (string playerID, string playerName, string gameCode);
    void PlayerLeave (string playerID);
    
	void PlayerMovement (string playerID, MoveDirection moveDirection);
    
    void PlayerUseSpecial(string playerID, SpecialType specialType);
}