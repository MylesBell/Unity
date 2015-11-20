using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class UnitFactory: NetworkBehaviour {
    
	public GameObject CreateGrunt(GameObject prefab, Vector3 basePosition) {
		GameObject gruntObject = Instantiate (prefab, basePosition, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (gruntObject);
		//grunt grunt = gruntobject.getcomponent<grunt> ();
		//grunt.initialisegrunt (teamid, desiredposition, channelseparation);
		return gruntObject;
	}

	public GameObject CreateHero(GameObject prefab, Team team, string playerName, Vector3 spawnLocation, Vector3 desiredPosition, float channelSeparation) {
		GameObject heroObject = Instantiate (prefab, spawnLocation, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (heroObject);
		Hero hero = heroObject.GetComponent<Hero> ();
		hero.InitialiseHero (team, playerName, spawnLocation, desiredPosition, channelSeparation);
		return heroObject;
	}

    public GameObject CreateBase(GameObject prefab, TeamID teamID, Vector3 spawnLocation) {
        GameObject baseTower = (GameObject)Instantiate(prefab, spawnLocation, Quaternion.identity);
        baseTower.GetComponent<Base>().initialise(teamID);
        NetworkServer.Spawn(baseTower);
        return baseTower;
    }
}
