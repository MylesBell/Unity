using UnityEngine;

public class WaterKills : MonoBehaviour {

	void OnCollisionEnter (Collision col) {
        if(col.gameObject.GetComponent<Hero>() && col.gameObject.GetComponent<Hero>().isServer
        || col.gameObject.GetComponent<Grunt>() && col.gameObject.GetComponent<Grunt>().isServer){
            float currenthealth = col.gameObject.GetComponent<Health>().currentHealth;
            bool killedPlayer;
            col.gameObject.GetComponent<Health>().ReduceHealth(currenthealth + 2f, out killedPlayer);
            if(killedPlayer) Debug.Log("Water kills");
        }
    }
}