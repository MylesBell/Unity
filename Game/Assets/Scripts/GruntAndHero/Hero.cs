using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement {
    [SyncVar] public string HeroNameString = "";
    private TeamID teamID;
	private TargetSelect targetSelect;

    public void Start() {
        GameObject heroname = transform.FindChild("HeroName").gameObject;
        heroname.GetComponent<TextMesh>().text = HeroNameString;
    }
    
	public void InitialiseHero(TeamID teamIDInput, string playerName, Channel channelInput, Vector3 channelTarget, Vector3 channelOffset) {
        teamID = teamIDInput;
        GameObject heroname = transform.FindChild("HeroName").gameObject;
        HeroNameString = playerName;
        heroname.GetComponent<TextMesh>().text = HeroNameString;
        if (isServer) {
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (teamIDInput, channelInput, channelTarget, channelOffset);
        }
	}

    void onDestroy() {
        //fire event to SocketIo that hero is dead
    }

    public TeamID getTeamID()
    {
        return teamID;
    }

    public string getHeroName()
    {
        return HeroNameString;
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
