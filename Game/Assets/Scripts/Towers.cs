using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Towers : NetworkBehaviour {

	public GameObject towerPrefab;	
	private List<GameObject> towers = new List<GameObject>();
	
	public void Initialise(int numScreensLeft, int numScreensRight, Team blueTeam, Team redTeam){
		
		for (int i = 0; i < numScreensLeft; i++){
			if (IsTower(numScreensLeft, i)){
				GameObject tower = gameObject.GetComponent<UnitFactory>().CreateTower(towerPrefab);
				tower.GetComponent<Tower>().Initialise(ComputerLane.LEFT, blueTeam, redTeam);
				// x set depending on screen number, left lane so z 250
				tower.transform.position = new Vector3((i*100)+50,0,250);
				towers.Add(tower);
			}
		}
		
		for (int i = 0; i < numScreensRight; i++){
			if (IsTower(numScreensLeft, i)){
				GameObject tower = gameObject.GetComponent<UnitFactory>().CreateTower(towerPrefab);
				tower.GetComponent<Tower>().Initialise(ComputerLane.RIGHT, blueTeam, redTeam);
				// x set depending on screen number, right lane so z 50
				tower.transform.position = new Vector3((i*100)+50,0,50);
				towers.Add(tower);
			}
		}
	}	
	
	public void ResetTowers(){
		foreach (GameObject tower in towers){
			tower.GetComponent<Tower>().ResetTower();
		}
	}
	
    private bool IsTower(int numScreens, int screenNum) {
        // Calculating whether to have a tunnel screen or not
		bool isTower;
        if (numScreens % 2 == 0) {
            // Even number of screens
            if (screenNum < (numScreens/2))
                // Less than halfway point so count from start
                isTower = screenNum % 2 == 0;
            else
                // Over halfway point so count from end
                isTower = (numScreens - 1 - screenNum) % 2 == 0;
        } else {
            // Odd number of screens so every other screen is fine
            isTower = screenNum % 2 == 0;
        }
		// dont show base screen
		if (screenNum == 0 || screenNum == numScreens){
			// isTower = false;
		}
		Debug.Log(isTower);
        return isTower;
    }
}
