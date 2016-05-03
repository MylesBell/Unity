using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MusicScreenController : NetworkBehaviour {
    
    public AudioClip baseClip;
    public AudioClip[] MusicClips;
    public float crossFadeDuration;
    

    private int redHeroCount;
    private int redGruntCount;
    private int blueHeroCount;
    private int blueGruntCount;
    
    private AudioSource baseClipAudioSource;
    private AudioSource[] audioSources;
    private int defaultClipIndex;
    private int previousClipIndex;
    private int playingClipIndex;
    
    private int middleTrack;
    private bool isCrossFadeRunning = false;
    private Coroutine CrossFadeCoroutine;
    
    private bool musicStarted = false;
    
    private System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
    
	void Start () {
        if(!isServer) {
            redHeroCount = 0;
            redGruntCount = 0;
            blueHeroCount = 0;
            blueGruntCount = 0;
            gameObject.transform.position = Camera.main.transform.position;
            //set up base sound clip audio source
            baseClipAudioSource = gameObject.AddComponent<AudioSource>();
            baseClipAudioSource.loop = true;
            baseClipAudioSource.playOnAwake = false;
            baseClipAudioSource.Stop(); 
            baseClipAudioSource.clip = baseClip;
            baseClipAudioSource.volume = 0.5f;
            audioSources = new AudioSource[MusicClips.Length];
            for (int i = 0; i < MusicClips.Length; i++) {
                audioSources[i] = gameObject.AddComponent<AudioSource>();
                audioSources[i].loop = true;
                audioSources[i].playOnAwake = false;
                audioSources[i].Stop(); 
                audioSources[i].clip = MusicClips[i];
                audioSources[i].volume = 0f;
            }
            //Initially some screens will not have units on them, so play default music depending on screen position
            int numScreensLeft = GraniteNetworkManager.numberOfScreens_left;
            int numScreensRight = GraniteNetworkManager.numberOfScreens_right;
            int screenNumber = GraniteNetworkManager.screeNumber;
            int numScreens = GraniteNetworkManager.lane == ComputerLane.LEFT ? numScreensLeft : numScreensRight;
            if(audioSources.Length > 0) {
                defaultClipIndex = screenNumber < numScreens / 2 ? 0 : MusicClips.Length - 1;
                middleTrack = screenNumber < numScreens / 2 ? (MusicClips.Length/2) - 1 : MusicClips.Length/2;
                playingClipIndex = defaultClipIndex;
                audioSources[defaultClipIndex].volume = 1f;
            }
        }
	}
    
    public void StartMusic(float timeOffset){
        Debug.Log("Server starting Music");
        double startMusicTimestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds + timeOffset;
        RpcStartMusicLoops(startMusicTimestamp);
    }
    
	void Update () {
        if(musicStarted && Input.GetKeyDown(KeyCode.M)){
            RpcStopMusicLoops();
        } else if(!musicStarted && Input.GetKeyDown(KeyCode.M) && GameState.gameState == GameState.State.PLAYING){
            StartMusic(3f);
        }
        if(musicStarted && !isServer) {
            if(audioSources.Length == 0) {
                int redCount = redGruntCount + redHeroCount;
                int blueCount = blueGruntCount + blueHeroCount;
                // Debug.Log("Reds " + redCount + " blues " + blueCount);
                int newClipIndex;
                if(redCount == 0 && blueCount == 0) newClipIndex = playingClipIndex; //neither of them present, hence keep playing tunes
                else if(redCount == 0) newClipIndex = 0; //only blue present, play their tune
                else if(blueCount == 0) newClipIndex = MusicClips.Length - 1; //only red present, play their tune
                else if(redCount == blueCount) newClipIndex = middleTrack; //even number of each, play "equal" tunes
                else {
                    //agree about number of tracks
                    //then create ranges for 1/abs(redCount-blueCount) which point to a track taking the diff into account
                    if(redCount > blueCount) newClipIndex = MusicClips.Length - 1;
                    else                     newClipIndex = 0;
                }
                if(newClipIndex != playingClipIndex){
                    if(isCrossFadeRunning) {
                        StopCoroutine(CrossFadeCoroutine);
                        audioSources[previousClipIndex].volume = 0f;
                    }
                    CrossFadeCoroutine = StartCoroutine(CrossFade(audioSources[playingClipIndex], audioSources[newClipIndex], crossFadeDuration));
                    previousClipIndex = playingClipIndex;
                    playingClipIndex = newClipIndex;
                }
            }
        }
	}
    
    private IEnumerator CrossFade(AudioSource currentAudioSource, AudioSource newAudioSource, float duration){
        Debug.Log("Starting Music");
        isCrossFadeRunning = true;
        float fTimeCounter = 0f;
        float startVolume = currentAudioSource.volume;
        while (!(Mathf.Approximately(fTimeCounter, duration))) {
            fTimeCounter = Mathf.Clamp01(fTimeCounter + Time.deltaTime);
            currentAudioSource.volume = Mathf.Clamp01(startVolume - fTimeCounter);
            newAudioSource.volume = fTimeCounter;
            yield return new WaitForSeconds(0.02f);
        }
        isCrossFadeRunning = false;
    }
    
    [ClientRpc]
    public void RpcStartMusicLoops(double startTimestamp) {
        if(!isServer) StartMusicLoops(startTimestamp);
        musicStarted = true;
    }
    
    [ClientRpc]
    public void RpcStopMusicLoops() {
        if(!isServer) StopMusicLoops();
        musicStarted = false;
    }
    void StartMusicLoops(double startTimestamp) {
        double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
        baseClipAudioSource.PlayDelayed((float)(startTimestamp - timestamp));
        foreach (AudioSource audioSource in audioSources) {
            timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
            audioSource.PlayDelayed((float)(startTimestamp - timestamp));
        }
    }
    
    void StopMusicLoops() {
        baseClipAudioSource.Stop();
        foreach (AudioSource audioSource in audioSources) {
            audioSource.Stop();
        }
    }

    public void IncrementCount(bool isHero, TeamID teamID){
        switch(teamID){
            case TeamID.blue:
                if(isHero) blueHeroCount++;
                else       blueGruntCount++;
                break;
            case TeamID.red:
                if(isHero) redHeroCount++;
                else       redGruntCount++;
                break;
        }
    }
    public void DecrementCount(bool isHero, TeamID teamID){
        switch(teamID){
            case TeamID.blue:
                if(isHero) blueHeroCount--;
                else       blueGruntCount--;
                break;
            case TeamID.red:
                if(isHero) redHeroCount--;
                else       redGruntCount--;
                break;
        }
    }
    
}
