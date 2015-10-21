using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IPlayerJoin : IEventSystemHandler {
	void PlayerJoin();
}