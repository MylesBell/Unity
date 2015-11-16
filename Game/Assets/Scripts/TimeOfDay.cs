using UnityEngine;
using System.Collections;

public class TimeOfDay : MonoBehaviour {

	public float timeFactor = 2.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.right * Time.deltaTime * timeFactor, Space.World);
	}
}
