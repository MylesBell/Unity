﻿using UnityEngine.EventSystems;

public interface IHeroMovement : IEventSystemHandler {
	void PlayerBack ();
	void PlayerMoveLane(Direction direction);
}