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
	public void InitialiseHero(TeamID teamIDInput, string playerName, Vector3 desiredPosition, float channelOffset) {
        GameObject heroname = transform.FindChild("HeroName").gameObject;
        HeroNameString = playerName;
        heroname.GetComponent<TextMesh>().text = HeroNameString;
        if (isServer) {
            teamID = teamIDInput;
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
            targetSelect = GetComponent<TargetSelect> ();
            targetSelect.InitialiseTargetSelect (teamIDInput, desiredPosition, channelOffset);
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
	public void PlayerMoveChannel (MoveDirection moveDirection)
	{
        if (isServer) {
            targetSelect.MoveToZOffset(moveDirection);
        }
	}
	#endregion
}
