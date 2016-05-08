public interface ISocketIOInputEvents{
	void PlayerJoin (string playerID, string playerName, int playerClass, string gameCode);
    void PlayerLeave (string playerID);
    
    void ServerDisconnect();
    
	void PlayerMovement (string playerID, MoveDirection moveDirection);
    
    void PlayerUseSpecial(string playerID, SpecialType specialType);
}