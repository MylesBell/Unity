using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour {

	private TargetSelect targetSelect;

	public void InitialiseGrunt(TeamID teamIDInput, Channel channelInput, Vector3 channelTarget, Vector3 channelOffset) {
        if (isServer) { 
		    targetSelect = GetComponent<TargetSelect> ();
		    targetSelect.InitialiseTargetSelect (teamIDInput, channelInput, channelTarget, channelOffset);
        }
	}
}