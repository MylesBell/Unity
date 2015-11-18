using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement {
	private TargetSelect targetSelect;

	public void InitialiseHero(TeamID teamIDInput, Channel channelInput, Vector3 channelTarget, Vector3 channelOffset) {
        if (isServer) {
            targetSelect = GetComponent<TargetSelect>();
            targetSelect.InitialiseTargetSelect(teamIDInput, channelInput, channelTarget, channelOffset);
        }
	}
	
	#region IHeroMovement implementation
	public void PlayerBack ()
	{
		throw new System.NotImplementedException ();
	}
	public void PlayerMoveChannel (Channel channel)
	{
        if (isServer) {
            targetSelect.MoveToChannel(channel);
        }
	}
	#endregion
}
