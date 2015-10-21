using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public interface IHeroMovement : IEventSystemHandler {
	void Move(float x, float y);
}
