public interface ISocketIOInputEvents{
	void PlayerJoin (int playerID);
	void PlayerBack (int playerID);
	void PlayerMoveChannel (int playerID, Channel channel);
}