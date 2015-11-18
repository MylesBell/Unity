public interface ISocketIOInputEvents{
	void PlayerJoin (string playerID, string playerName);
	void PlayerBack (string playerID);
	void PlayerMoveChannel (string playerID, Channel channel);
}