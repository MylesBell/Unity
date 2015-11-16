using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Grunt : NetworkBehaviour {

	private TargetSelect targetSelect;

	public void InitialiseGrunt(TeamID teamIDInput, Channel channelInput, Vector3 channelTarget, Vector3 channelOffset) {
		SetTeam (teamIDInput);
		targetSelect = GetComponent<TargetSelect> ();
		targetSelect.InitialiseTargetSelect (teamIDInput, channelInput, channelTarget, channelOffset);
	}

	private void SetTeam(TeamID id) {
		Material mat = GetComponent<Renderer> ().material;
		if (id == TeamID.red) {
			mat.SetColor ("_Color", Color.red);
		} else {
			mat.SetColor ("_Color", Color.blue);
		}
	}
}