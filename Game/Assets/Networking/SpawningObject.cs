using UnityEngine;
using UnityEngine.Networking;

public class SpawningObject : NetworkBehaviour {

    public GameObject PlayerPrefab;
    
    void Start()
    {

    }
    public void Spawn()
    {
        GameObject player = (GameObject)Instantiate(PlayerPrefab, new Vector3(50,10,50), transform.rotation);
        NetworkServer.Spawn(player);
    }

    // Update is called once per frame
    void Update () {
        if (isServer)
        {
            if (Input.GetKey(KeyCode.C))
            {
                Spawn();
                Debug.Log("Spawned");
            }
        }
    }
}
