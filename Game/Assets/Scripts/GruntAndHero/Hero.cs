using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement {
	private TargetSelect targetSelect;
    //private GameObject 

	public void InitialiseHero(TeamID teamIDInput, string playerName, Channel channelInput, Vector3 channelTarget, Vector3 channelOffset) {
		targetSelect = GetComponent<TargetSelect> ();
		targetSelect.InitialiseTargetSelect (teamIDInput, channelInput, channelTarget, channelOffset);
        GameObject heroname = transform.FindChild("HeroName").gameObject;
        heroname.GetComponent<TextMesh>().text = playerName;
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
