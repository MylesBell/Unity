using UnityEngine;
using UnityEngine.EventSystems;

public interface IDestroyableGameObject : IEventSystemHandler {

    void InitialiseGameObject(Team team);
    void ResetGameObject(Vector3 spawnLocation, ComputerLane computerLane);
    void DisableGameObject();
}
