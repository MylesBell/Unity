using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class UnitFactory: NetworkBehaviour {
	
	public GameObject teamsObject, redBasePrefab, blueBasePrefab, blueHeroPrefab, redHeroPrefab, blueGruntPrefab, redGruntPrefab;

    
	public GameObject CreateGrunt(TeamID teamID, Channel channel, Vector3 spawnLocation, Vector3 channelTarget, float channelSeparation) {
		GameObject gruntPrefab = teamID == TeamID.blue ? blueGruntPrefab : redGruntPrefab;
		GameObject gruntObject = Instantiate (gruntPrefab, spawnLocation, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (gruntObject);
		Grunt grunt = gruntObject.GetComponent<Grunt> ();
		grunt.InitialiseGrunt (teamID, channel, channelTarget, channelSeparation);
		return gruntObject;
	}

	public GameObject CreateHero(TeamID teamID, string playerName, Channel channel, Vector3 spawnLocation, Vector3 channelTarget, float channelSeparation) {
		GameObject heroPrefab = teamID == TeamID.blue ? blueHeroPrefab : redHeroPrefab;
		GameObject heroObject = Instantiate (heroPrefab, spawnLocation, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (heroObject);
		Hero hero = heroObject.GetComponent<Hero> ();
		hero.InitialiseHero (teamID, playerName, channel, channelTarget, channelSeparation);
		return heroObject;
	}

    public GameObject CreateBase(TeamID teamID, Vector3 spawnLocation) {
        GameObject basePrefab = teamID == TeamID.blue ? blueBasePrefab : redBasePrefab;
        GameObject baseTower = (GameObject)Instantiate(basePrefab, spawnLocation, Quaternion.identity);
        baseTower.GetComponent<Base>().initialise(teamID);
        NetworkServer.Spawn(baseTower);
        return baseTower;
    }
}
