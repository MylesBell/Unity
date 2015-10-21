using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour, IHeroMovement {

	public float speed = 3.0f;
	public float health = 10.0f;
	public TeamID teamID;
	// Rigidbody controls physical properties of object
	private Rigidbody rigidBody;
	
	void Start () {
		rigidBody = GetComponent<Rigidbody>();
	}

	public void InitialiseHero(TeamID id) {
		SetTeam (id);
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

	public void Move (float x, float y) {
		Vector3 movement = new Vector3 (x, 0.0f, y);
		
		rigidBody.AddForce (movement * speed);
	}

	#endregion
}
