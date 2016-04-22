using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Towers : NetworkBehaviour {

	public GameObject towerPrefab;
	
	public void Initialise(bool hasLeftLane, bool hasRightLane){
		GameObject tower = gameObject.GetComponent<UnitFactory>().CreateTower(towerPrefab);
        tower.GetComponent<Tower>().Initialise(ComputerLane.LEFT);
        tower.transform.position = new Vector3(50,0,50);
	}
}
