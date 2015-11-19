using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class UnitFactory: NetworkBehaviour {
    
	public GameObject CreateGrunt(GameObject prefab, TeamID teamID, Vector3 spawnLocation, Vector3 desiredPosition, float channelSeparation) {
		GameObject gruntObject = Instantiate (prefab, spawnLocation, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (gruntObject);
		Grunt grunt = gruntObject.GetComponent<Grunt> ();
		grunt.InitialiseGrunt (teamID, desiredPosition, channelSeparation);
		return gruntObject;
	}

	public GameObject CreateHero(GameObject prefab, TeamID teamID, string playerName, Vector3 spawnLocation, Vector3 desiredPosition, float channelSeparation) {
		GameObject heroObject = Instantiate (prefab, spawnLocation, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (heroObject);
		Hero hero = heroObject.GetComponent<Hero> ();
		hero.InitialiseHero (teamID, playerName, desiredPosition, channelSeparation);
		return heroObject;
	}

    public GameObject CreateBase(GameObject prefab, TeamID teamID, Vector3 spawnLocation) {
        GameObject baseTower = (GameObject)Instantiate(prefab, spawnLocation, Quaternion.identity);
        baseTower.GetComponent<Base>().initialise(teamID);
        NetworkServer.Spawn(baseTower);
        return baseTower;
    }
}
