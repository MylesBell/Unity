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
        GameObject baseTower = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(baseTower);
        return baseTower;
    }
	
	public GameObject CreateTower(GameObject prefab) {
		GameObject tower = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(tower);
        return tower;
	}
}
