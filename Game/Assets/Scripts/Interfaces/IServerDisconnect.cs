using UnityEngine.EventSystems;

public interface IServerDisconnect : IEventSystemHandler {
	void ServerDisconnect();
}