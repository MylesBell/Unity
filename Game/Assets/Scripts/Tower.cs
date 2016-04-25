using UnityEngine;
using UnityEngine.Networking;

public enum TowerState {
	red, neutral, blue
}

public enum HeroCapturing{
	red, blue, both, none
}

public class Tower : NetworkBehaviour {
    [SyncVar] private bool active = true;
	
	[SyncVar] private float percentRed;
	[SyncVar] private float percentBlue;
	[SyncVar] private float captureBarHeight;
	[SyncVar] private float yOffset;
	private TowerState towerState;
	
	public Texture captureBarRedTexture, captureBarBlueTexture, captureBarBackTexture;
	public float captureBarInitialLength;
	private float captureBarLength;
    public float captureBarOffset;
	
	private Vector3 entityLocation;
	public float captureRadius;
	public float captureRate;
	public float gruntSpawnInterval;
	private float nextGruntRespawn;
	
	public GameObject neutralTower;
	public GameObject cowboyTower;
	public GameObject vikingTower;
	
	private ComputerLane computerLane;
    
    public void Initialise(ComputerLane computerlane) {
        // set initial colours
		percentRed = 0f;
		percentBlue = 0f;
		towerState = TowerState.neutral;
		RpcSetTowerState(towerState);
		captureBarHeight = (Screen.height / 150) < 3? 3 : Screen.height / 150;
		captureBarHeight -= captureBarHeight % 3;
		captureBarHeight = 5 * captureBarHeight / 3;
		yOffset = captureBarOffset * captureBarHeight;
		
        entityLocation = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        captureBarLength = (percentRed / 100) * captureBarInitialLength;
		this.computerLane = computerlane;
    }

	void OnGUI () {
		// gather required bar position values
		float length = captureBarInitialLength * captureBarHeight;
		float xPos = entityLocation.x - (length / 2) - (captureBarHeight / 5);
		float yPos = Screen.height - entityLocation.y - yOffset - (captureBarHeight / 5);
		
		GUI.DrawTexture(new Rect(xPos, yPos,length + (2 * captureBarHeight / 5), captureBarHeight), captureBarBackTexture);
		
		// when red or blue has any capture
		if (percentRed > 0 || percentBlue > 0) {
			float captureBarX;
            Texture captureBarTexture = captureBarRedTexture;
			// set texture and side to draw on depending on computerlane
			captureBarX = entityLocation.x - (length/2);
			if (percentRed > 0){
				captureBarTexture = captureBarRedTexture;
				if (computerLane == ComputerLane.RIGHT){
					captureBarX = entityLocation.x + (length/2) - (captureBarLength * captureBarHeight);	
				}
			// when blue draw from right
			}else{
				captureBarTexture = captureBarBlueTexture;
				if (computerLane == ComputerLane.LEFT){
					captureBarX = entityLocation.x + (length/2) - (captureBarLength * captureBarHeight);	
				}
			}
			
			GUI.DrawTexture(new Rect(captureBarX, Screen.height - entityLocation.y - yOffset,
									captureBarLength * captureBarHeight, 3 * captureBarHeight / 5), 
									captureBarTexture);
		}
	}
	
	// Update is called once per frame
	void Update () {
		// update capture values
		if (isServer){
			UpdateCaptureValues();
		}
		
		// update draw location
		entityLocation =  Camera.main.WorldToScreenPoint(gameObject.transform.position);
		// update bar length depending who has captured
		if (percentRed > 0){
			captureBarLength = (percentRed / 100) * captureBarInitialLength;
		}else if(percentBlue > 0){
			captureBarLength = (percentBlue / 100) * captureBarInitialLength;
		}
	}
	
	private void UpdateCaptureValues(){
		HeroCapturing heroCapturing = CmdHeroesCapturing(captureRadius);

		// if red capturing
		if (towerState != TowerState.red && heroCapturing == HeroCapturing.red){
			if (percentBlue > 0){
				percentBlue -= captureRate;
			}else{
				percentRed += captureRate;
			}
		// if blue capturing
		}else if(towerState != TowerState.blue && heroCapturing == HeroCapturing.blue){
			if (percentRed > 0){
				percentRed -= captureRate;
			}else{
				percentBlue += captureRate;
			}
		// when left tend to current state
		}else if (heroCapturing == HeroCapturing.none)
			if (towerState == TowerState.red && percentRed < 100f) percentRed += (captureRate * 0.5f);
			if (towerState == TowerState.blue && percentBlue < 100f) percentBlue += (captureRate * 0.5f);
			if (towerState == TowerState.neutral){
				if (percentRed > 0) percentRed -= (captureRate * 0.25f);
				if (percentBlue > 0) percentBlue -= (captureRate * 0.25f);
		}
		
		// update capture value and rpc if changed
		TowerState oldTowerState = towerState;
		if (percentRed >= 100f){
			towerState = TowerState.red;
			percentRed = 100f;
		}else if (percentBlue >= 100f){
			towerState = TowerState.blue;
			percentBlue = 100f;
		}else if ((percentRed <= 0 && percentBlue <= 0)){
			towerState = TowerState.neutral;
			percentRed = 0;
			percentBlue = 0;
		}
		if (towerState != oldTowerState) RpcSetTowerState(towerState);
	}

	[ClientRpc]
	private void RpcSetTowerState(TowerState towerState){
		Debug.Log("tower captured: " + towerState);
		if (towerState == TowerState.neutral){
			neutralTower.SetActive(true);
			cowboyTower.SetActive(false);
			vikingTower.SetActive(false);
		}else if (towerState == TowerState.red){
			neutralTower.SetActive(false);
			cowboyTower.SetActive(false);
			vikingTower.SetActive(true);
		}else{
			neutralTower.SetActive(false);
			cowboyTower.SetActive(true);
			vikingTower.SetActive(false);
		}
		this.towerState = towerState;
	}
	
	// Get if any capturing happening
    private HeroCapturing CmdHeroesCapturing(float radius){
		// get heros in area
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
		bool redHeroPresent = false;
		bool blueHeroPresent = false;
        foreach(Collider collider in colliders) {
            if (collider.gameObject.tag.Equals("redHero")) redHeroPresent = true;
            if (collider.gameObject.tag.Equals("blueHero")) blueHeroPresent = true;
        }
		
		// set who is capturing
		HeroCapturing heroCapturing = HeroCapturing.none;
		if (redHeroPresent && !blueHeroPresent) heroCapturing = HeroCapturing.red;
		if (!redHeroPresent && blueHeroPresent) heroCapturing = HeroCapturing.blue;
		if (redHeroPresent && blueHeroPresent) heroCapturing = HeroCapturing.both;
		
		return heroCapturing;
    }
	
	public void ResetTower(){
		percentRed = 0f;
		percentBlue = 0f;
		towerState = TowerState.neutral;
		RpcSetTowerState(towerState);
	}
	
    public void DisableGameObject() {
        CmdSetActiveState(false);
    }
	
    [Command]
    public void CmdSetActiveState(bool active) {
        RpcSetActive(active);
    }

    [ClientRpc]
    public void RpcSetActive(bool active) {
        gameObject.SetActive(active);
    }
}
