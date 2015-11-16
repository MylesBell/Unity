using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement {
	private TargetSelect targetSelect;

	public void InitialiseHero(TeamID teamIDInput, Channel channelInput, Vector3 channelTarget, Vector3 channelOffset) {
		targetSelect = GetComponent<TargetSelect> ();
		targetSelect.InitialiseTargetSelect (teamIDInput, channelInput, channelTarget, channelOffset);
	}
	
	#region IHeroMovement implementation
	public void PlayerBack ()
	{
		throw new System.NotImplementedException ();
	}
	public void PlayerMoveLane (Channel channel)
	{
		throw new System.NotImplementedException ();
	}
	#endregion
}
