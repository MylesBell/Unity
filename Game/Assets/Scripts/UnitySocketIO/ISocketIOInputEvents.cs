public interface ISocketIOInputEvents{
	void PlayerJoin (int playerID, string playerName);
	void PlayerBack (int playerID);
	void PlayerMoveChannel (int playerID, Channel channel);
}