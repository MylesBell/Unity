using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement {
	private TargetSelect targetSelect;

	public void InitialiseHero(TeamID teamIDInput, Channel channelInput, Vector3 channelTarget, Vector3 channelOffset) {
		SetTeam (teamIDInput);
		targetSelect = GetComponent<TargetSelect> ();
		targetSelect.InitialiseTargetSelect (teamIDInput, channelInput, channelTarget, channelOffset);
	}
	
	private void SetTeam(TeamID id) {
		Material mat = GetComponent<Renderer> ().material;
		Light light = transform.GetComponentInChildren<Light> ();
		if (id == TeamID.red) {
			mat.SetColor ("_Color", Color.red);
			mat.SetColor("_EmissionColor", Color.red);
			light.color = Color.red;
		} else {
			mat.SetColor ("_Color", Color.blue);
			mat.SetColor("_EmissionColor", Color.blue);
			light.color = Color.blue;
			
		}
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
