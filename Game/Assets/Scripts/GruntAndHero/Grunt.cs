using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour {

	private TargetSelect targetSelect;

	public void InitialiseGrunt(TeamID teamIDInput, Channel channelInput, Vector3 channelTarget, float channelOffset) {
        if (isServer) {
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
            targetSelect = GetComponent<TargetSelect> ();
		    targetSelect.InitialiseTargetSelect (teamIDInput, channelInput, channelTarget, channelOffset);
        }
	}
}