public interface ISocketIOInputEvents{
	void PlayerJoin (string playerID, string playerName, string gameCode);
    void PlayerLeave (string playerID);
    
	void PlayerMovement (string playerID, MoveDirection moveDirection);
	void PlayerStopMovement (string playerID);
    
    void PlayerUseSpecial(string playerID, SpecialType specialType);
    
	void PlayerSwitchBase (string playerID);
}