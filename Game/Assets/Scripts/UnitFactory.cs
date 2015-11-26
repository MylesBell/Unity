using UnityEngine;
using UnityEngine.Networking;

public class UnitFactory: NetworkBehaviour {
    
	public GameObject CreateGrunt(GameObject prefab) {
		GameObject gruntObject = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (gruntObject);
		return gruntObject;
	}

	public GameObject CreateHero(GameObject prefab) {
		GameObject heroObject = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (heroObject);
		return heroObject;
	}

    public GameObject CreateBase(GameObject prefab) {
        GameObject baseTower = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(baseTower);
        return baseTower;
    }
}
