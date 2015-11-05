using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {
	public static float maxHealth = 100.0f;
	public float healthBarInitialLength;
	public Texture healthBarTexture;
	private float currentHealth = maxHealth;
	private float healthBarLength;
	private float percentOfHealth;
	private Vector3 entityLocation;
	
	void OnGUI () {
		if (currentHealth > 0) {
			GUI.DrawTexture(new Rect(entityLocation.x - 5 ,
			                         Screen.height - entityLocation.y - 10,
			                         healthBarLength, 2), healthBarTexture);
		}
	}
	
	void Update () {
		entityLocation =  Camera.main.WorldToScreenPoint(gameObject.transform.position);
		if(Input.GetKeyUp (KeyCode.Q)) {
			currentHealth -= 10.0f;
		}
		if (currentHealth <= 0) {
			Destroy(gameObject);
		}
		percentOfHealth = currentHealth / maxHealth;
		healthBarLength = percentOfHealth * 10;
	}
}