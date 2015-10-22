using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpawningObject : NetworkBehaviour {

    public GameObject PlayerPrefab;

    private bool isServer;
    void Start()
    {
        isServer = PlayerPrefs.GetInt("isServer", 1) == 1 ? true : false;
    }
    public void Spawn()
    {
        GameObject player = (GameObject)Instantiate(PlayerPrefab, new Vector3(400,10,300), transform.rotation);
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
