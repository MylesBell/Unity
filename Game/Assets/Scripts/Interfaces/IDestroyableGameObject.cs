using UnityEngine;
using UnityEngine.EventSystems;

public interface IDestroyableGameObject : IEventSystemHandler {

    void InitialiseGameObject(Team team);
    void ResetGameObject(Vector3 spawnLocation, Vector3 desiredPosition, float channelOffset);
    void DisableGameObject();
}
