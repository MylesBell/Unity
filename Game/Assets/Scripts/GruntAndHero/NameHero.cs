using UnityEngine;

public class NameHero : MonoBehaviour {
	// Update is called once per frame
    //prevent the mesh from rotating with the character
	void Update ()
    {
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
}
