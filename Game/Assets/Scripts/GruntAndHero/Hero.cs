using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement {
    [SyncVar] private string HeroNameString = "";
	private TargetSelect targetSelect;

    public void Start() {
        GameObject heroname = transform.FindChild("HeroName").gameObject;
        heroname.GetComponent<TextMesh>().text = HeroNameString;
    }
    
	public void InitialiseHero(TeamID teamIDInput, string playerName, Channel channelInput, Vector3 channelTarget, float channelOffset) {
        GameObject heroname = transform.FindChild("HeroName").gameObject;
        HeroNameString = playerName;
        heroname.GetComponent<TextMesh>().text = HeroNameString;
        if (isServer) {
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (teamIDInput, channelInput, channelTarget, channelOffset);
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
