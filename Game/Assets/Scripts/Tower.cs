using UnityEngine;
using UnityEngine.Networking;

public enum TowerState {
	red, neutral, blue
}

public enum HeroCapturing{
	red, blue, both, none
}

public class Tower : NetworkBehaviour {
	
	[SyncVar] private float percentRed;
	[SyncVar] private float percentBlue;
	[SyncVar] private float captureBarHeight;
	[SyncVar] private float yOffset;
	private TowerState towerState;
	
	public Texture captureBarRedTexture, captureBarBlueTexture, captureBarBackTexture;
	public float captureBarInitialLength = 20.0f;
	private float captureBarLength;
    public float captureBarOffset = 5.0f;
	
	private Vector3 entityLocation;
	public float captureRadius = 10.0f;
	public float captureRate = 0.25f;
	public float gruntSpawnInterval = 0;
	private float nextGruntRespawn;
    
    public void InitialiseTower(ComputerLane computerlane) {
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
    }

	void OnGUI () {
		// gather required bar position values
		float length = captureBarInitialLength * captureBarHeight;
		float xPos = entityLocation.x - (length / 2) - (captureBarHeight / 5);
		float yPos = Screen.height - entityLocation.y - yOffset - (captureBarHeight / 5);
		
		GUI.DrawTexture(new Rect(xPos, yPos,length, captureBarHeight), captureBarBackTexture);
		
		if (percentRed > 0 || percentBlue > 0) {
			float captureBarX;
            Texture captureBarTexture = captureBarRedTexture;
			if (percentRed > 0){
				captureBarX = entityLocation.x - (length/2);
				captureBarTexture = captureBarRedTexture;
			}else{
				captureBarX = entityLocation.x + (length/2) + (captureBarLength * captureBarHeight);
				captureBarTexture = captureBarBlueTexture;	
			}
			
			GUI.DrawTexture(new Rect(captureBarX, Screen.height - entityLocation.y - yOffset,
									captureBarLength * captureBarHeight, 3 * captureBarHeight / 5), 
									captureBarTexture);
		}
	}	
	
	// Update is called once per frame
	void Update () {
		// update capture values
		CmdUpdateCaptureValues();
		
		// update draw properties
		entityLocation =  Camera.main.WorldToScreenPoint(gameObject.transform.position);
		captureBarLength = (percentRed / 100) * captureBarInitialLength;
	}
	
	
	[Command]
	private void CmdUpdateCaptureValues(){
		HeroCapturing heroCapturing = CmdHeroesCapturing(captureRadius);

		// if red capturing
		if (towerState != TowerState.red && heroCapturing == HeroCapturing.red){
			if (towerState == TowerState.neutral){
				percentRed += captureRate;
			}else{
				percentBlue -= captureRate;
			}
		// if blue capturing
		}else if(towerState != TowerState.blue && heroCapturing == HeroCapturing.blue){
			if (towerState == TowerState.neutral){
				percentBlue += captureRate;
			}else{
				percentRed -= captureRate;
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
		if (percentRed == 100f) towerState = TowerState.red;
		if (percentBlue == 100f) towerState = TowerState.blue;
		if ((percentRed == 0 && percentBlue == 0)) towerState = TowerState.neutral;
		if (towerState != oldTowerState) RpcSetTowerState(towerState);
	}
	
	[ClientRpc]
	private void RpcSetTowerState(TowerState towerState){
		this.towerState = towerState;
	}
	
	// Get if heroes in area
    private HeroCapturing CmdHeroesCapturing(float radius){
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
		bool redHeroPresent = false;
		bool blueHeroPresent = false;
        foreach(Collider collider in colliders) {
            if (collider.gameObject.tag.Equals("redHero")) redHeroPresent = true;
            if (collider.gameObject.tag.Equals("blueHero")) blueHeroPresent = true;
        }
		
		HeroCapturing heroCapturing = HeroCapturing.none;
		if (redHeroPresent && !blueHeroPresent) heroCapturing = HeroCapturing.red;
		if (!redHeroPresent && blueHeroPresent) heroCapturing = HeroCapturing.blue;
		if (redHeroPresent && blueHeroPresent) heroCapturing = HeroCapturing.both;
		
		
		return heroCapturing;
    }
	
}
