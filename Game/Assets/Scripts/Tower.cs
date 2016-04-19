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
		
        entityLocation = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        captureBarLength = (percentRed / 100) * captureBarInitialLength;
    }

	void OnGUI () {
		if (percentRed > 0 || percentBlue > 0) {
            int captureBarHeight = (Screen.height / 150) < 3? 3 : Screen.height / 150;
            captureBarHeight -= captureBarHeight % 3;
			
            float length = captureBarInitialLength * captureBarHeight + (2 * captureBarHeight/3);
            float height = captureBarHeight + (2 * captureBarHeight / 3);
			float yOffset = captureBarOffset * height;
            float xPos = entityLocation.x - (length/2) - (captureBarHeight / 3);
            float yPos = Screen.height - entityLocation.y - yOffset - (captureBarHeight / 3);
            
			float captureBarX;
            Texture captureBarTexture = captureBarRedTexture;
			if (percentRed > 0){
				captureBarX = entityLocation.x - (length/2);
				captureBarTexture = captureBarRedTexture;
			}else{
				captureBarX = entityLocation.x + (length/2) + (captureBarLength * captureBarHeight);
				captureBarTexture = captureBarBlueTexture;	
			}
			
			GUI.DrawTexture(new Rect(xPos, yPos,length, height), captureBarBackTexture);
			GUI.DrawTexture(new Rect(captureBarX, Screen.height - entityLocation.y - yOffset,
									captureBarLength * captureBarHeight, captureBarHeight), 
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
		float lastPercentRed = percentRed;
		float lastPercentBlue = percentBlue;
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
		}
		
		// when left tend to current state
		if (towerState == TowerState.red && percentRed < 100f) percentRed += (captureRate * 0.5f);
		if (towerState == TowerState.blue && percentBlue < 100f) percentBlue += (captureRate * 0.5f);
		if (towerState == TowerState.neutral){
			if (percentRed > 0) percentRed -= (captureRate * 0.25f);
			if (percentBlue > 0) percentBlue -= (captureRate * 0.25f);
		}
		
		// update capture value
		if (percentRed == 100f) towerState = TowerState.red;
		if (percentBlue == 100f) towerState = TowerState.blue;
		if ((percentRed == 0 && lastPercentRed > 0) || (percentBlue == 0 && lastPercentBlue > 0)) towerState = TowerState.neutral;
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
