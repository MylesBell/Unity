using UnityEngine.EventSystems;

public interface IPlayerSpecial : IEventSystemHandler {
	
	void PlayerSpecial(SpecialType specialType);
}