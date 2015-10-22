using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour, IHeroMovement {
	public TeamID teamID;
	public float speed = 3.0f;
	public float health = 10.0f;
	public string playerType = "elf";
	// Rigidbody controls physical properties of object
	private Rigidbody rigidBody;
	
	void Start () {
		rigidBody = GetComponent<Rigidbody>();
	}

	public void InitialiseHero(TeamID teamID) {
		SetTeam (teamID);
	}

	void SetTeam(TeamID id) {
		teamID = id;
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
	public void PlayerBack (int playerID)
	{
		throw new System.NotImplementedException ();
	}
	public void PlayerMoveLane (Direction direction)
	{
		throw new System.NotImplementedException ();
	}
	#endregion
}
