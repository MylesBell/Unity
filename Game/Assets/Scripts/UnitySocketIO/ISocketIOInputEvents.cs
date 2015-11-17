public interface ISocketIOInputEvents{
	void PlayerJoin (string playerID);
	void PlayerBack (string playerID);
	void PlayerMoveChannel (string playerID, Channel channel);
}