
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public enum MoveDirection {
	E, SE, S, SW, W, NW, N, NE, NONE
}

public class HeroMovement : NetworkBehaviour, IHeroMovement
{
    public void PlayerMovement(MoveDirection moveDirection)
    {
        throw new NotImplementedException();
    }

}