using UnityEngine;
using System.Collections;

public class NameHero : MonoBehaviour {

    private Vector3 entityLocation;
    private TextMesh nameTextMesh;
    // Use this for initialization
    void Start () {
        nameTextMesh = gameObject.GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
    //prevent the mesh from rotating with the character
	void Update ()
    {
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
}
