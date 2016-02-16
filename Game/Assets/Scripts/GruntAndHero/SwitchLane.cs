using UnityEngine;
/*
Script attached to "doors" to invoke the switch lane
*/
public class SwitchLane : MonoBehaviour {

	void OnCollisionEnter (Collision col) {
        if(col.gameObject.GetComponent<Hero>() && col.gameObject.GetComponent<Hero>().isServer){
            Debug.Log("Calling lane switch");
            col.gameObject.GetComponent<Hero>().switchLane();
        }
    }
}
