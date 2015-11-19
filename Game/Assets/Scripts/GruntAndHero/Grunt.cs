using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour {

	private TargetSelect targetSelect;

	public void InitialiseGrunt(TeamID teamIDInput, Vector3 desiredPosition, float channelOffset) {
        if (isServer) {
            //set Health to Max
            gameObject.GetComponent<Health>().initialiseHealth();
            targetSelect = GetComponent<TargetSelect> ();
		    targetSelect.InitialiseTargetSelect (teamIDInput, desiredPosition, channelOffset);
        }
	}
}