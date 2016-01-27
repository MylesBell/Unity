using UnityEngine;

public class NameHero : MonoBehaviour {
	// Update is called once per frame
    //prevent the mesh from rotating with the character
    
    Vector3 fixedRotation;
	void Update ()
    {
        gameObject.transform.rotation = Quaternion.Euler(fixedRotation);
    }
    
    public void setTextRotation(Vector3 fixedRotation){
        this.fixedRotation = fixedRotation;
    }
}
