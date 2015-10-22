public interface ISocketIOInputEvents{
	void PlayerJoin (int playerID);
	void PlayerBack (int playerID);
	void PlayerMoveLane (int playerID, Direction direction);
}