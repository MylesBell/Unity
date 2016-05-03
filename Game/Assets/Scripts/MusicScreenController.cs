using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MusicScreenController : NetworkBehaviour {
    
    public AudioClip baseClip;
    public AudioClip[] MusicClips;
    
    public AudioClip cowboysOpeningClip;
    public AudioClip vikingOpeningClip;
    public float crossFadeDuration;
    

    private int vikingHeroCount;
    private int vikingGruntCount;
    private int cowboyHeroCount;
    private int cowboyGruntCount;
    
    private AudioSource baseClipAudioSource;
    private AudioSource[] audioSources;
    private AudioSource openingClipAudioSource;
    private float openingClipLength;
    private int defaultClipIndex;
    private int playingClipIndex;
    
    private int middleTrack;
    private bool isCrossFadeRunning = false;
    private bool musicStarted = false;
    private bool openingMusicPlayed = false;
    
    private System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
    
    private Queue<IEnumerator> crossFadeQueue;
    
	void Start () {
        if(!isServer) {
            crossFadeQueue = new Queue<IEnumerator>();
            vikingHeroCount = 0;
            vikingGruntCount = 0;
            cowboyHeroCount = 0;
            cowboyGruntCount = 0;
            gameObject.transform.position = Camera.main.transform.position;
            //set up base sound clip audio source
            baseClipAudioSource = gameObject.AddComponent<AudioSource>();
            baseClipAudioSource.loop = true;
            baseClipAudioSource.playOnAwake = false;
            baseClipAudioSource.Stop(); 
            baseClipAudioSource.clip = baseClip;
            baseClipAudioSource.volume = 0f;
            
            //set up all other sources
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
            }
            //set up opening call
            openingClipAudioSource = gameObject.AddComponent<AudioSource>();
            openingClipAudioSource.loop = false;
            openingClipAudioSource.playOnAwake = false;
            openingClipAudioSource.Stop(); 
            openingClipAudioSource.clip = screenNumber < numScreens / 2 ? cowboysOpeningClip : vikingOpeningClip;
            openingClipLength = screenNumber < numScreens / 2 ? cowboysOpeningClip.length : vikingOpeningClip.length;
            openingClipAudioSource.volume = 0f;
        }
	}
    
    public void StartMusic(float timeOffset){
        Debug.Log("Server starting Music");
        double startMusicTimestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds + timeOffset;
        RpcStartMusicLoops(startMusicTimestamp);
    }
    
    public void StopMusic(){
        RpcStopMusicLoops();
    }
    
	void Update () {
        if(musicStarted){
            if(isServer && Input.GetKeyDown(KeyCode.M)){
                RpcStopMusicLoops();
            } else if(!isServer && audioSources.Length > 0 && openingMusicPlayed){
                int vikingCount = vikingGruntCount + vikingHeroCount;
                int cowboysCount = cowboyGruntCount + cowboyHeroCount;
                int newClipIndex;
                if(vikingCount == 0 && cowboysCount == 0) newClipIndex = playingClipIndex; //neither of them present, hence keep playing tunes
                else if(vikingCount == 0) newClipIndex = 0; //only cowboys present, play their tune
                else if(cowboysCount == 0) newClipIndex = MusicClips.Length - 1; //only viking present, play their tune
                else if(vikingCount == cowboysCount) newClipIndex = middleTrack; //even number of each, play "equal" tunes
                else {
                    int diff = vikingCount - cowboysCount;
                     //0 is the most neutral and 4 is the highest
                    int songLevel = 0;
                    switch((int)Mathf.Abs(diff)){
                        case 1:
                            songLevel = 0;
                            break;
                        case 2:
                            songLevel = 1;
                            break;
                        case 3:
                        case 4:
                            songLevel = 2;
                            break;
                        case 5:
                        case 6:
                            songLevel = 3;
                            break;
                        default:
                            songLevel = 4;
                            break;
                    }
                    // Debug.Log("diff " + diff);
                    // Debug.Log("vikings " + vikingCount + " cowboyss " + cowboysCount);
                    // Debug.Log("Song Level " + songLevel);
                    newClipIndex = diff > 0 ? songLevel + 5 : 4 - songLevel;
                    // Debug.Log("New clip index " + newClipIndex);
                }
                if(newClipIndex != playingClipIndex){
                    //cross fade through all the tracks inbetween current and desired
                    if(playingClipIndex > newClipIndex){
                        for(int i = playingClipIndex; i > newClipIndex; i--){
                          crossFadeQueue.Enqueue(CrossFade(audioSources[i], audioSources[i-1], crossFadeDuration));
                        //   Debug.Log("Cross fading from " + i + " to " + (i-1));
                        }
                    } else {
                        for(int i = playingClipIndex; i < newClipIndex; i++){
                          crossFadeQueue.Enqueue(CrossFade(audioSources[i], audioSources[i+1], crossFadeDuration));
                        //   Debug.Log("Cross fading from " + i + " to " + (i+1));
                        }
                    }
                    playingClipIndex = newClipIndex;
                }
                startEnqueuedCrossFade();
                
            }
        } else if(!musicStarted && Input.GetKeyDown(KeyCode.M) && GameState.gameState == GameState.State.PLAYING){
            StartMusic(3f);
        }
	}
    private void startEnqueuedCrossFade(){
        if(!isCrossFadeRunning && crossFadeQueue.Count > 0){
            StartCoroutine(crossFadeQueue.Dequeue());
        }
    }
    private IEnumerator CrossFade(AudioSource currentAudioSource, AudioSource newAudioSource, float duration){
        isCrossFadeRunning = true;
        // Debug.Log("Running coroutine");
        float timeElapsed = 0f;
        float startVolume = currentAudioSource.volume;
        float step = 1f/duration;
        while (timeElapsed < duration) {
            timeElapsed += Time.deltaTime;
            currentAudioSource.volume = Mathf.Clamp01(startVolume - timeElapsed*step);
            newAudioSource.volume = timeElapsed*step;
            yield return new WaitForSeconds(0.01f);
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
        crossFadeQueue.Clear();
        isCrossFadeRunning = false;
        //start all the time loops
        double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
        baseClipAudioSource.PlayDelayed((float)(startTimestamp - timestamp));
        baseClipAudioSource.volume = 0.5f;
        foreach (AudioSource audioSource in audioSources) {
            timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
            audioSource.PlayDelayed((float)(startTimestamp - timestamp));
        }
        timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
        openingClipAudioSource.PlayDelayed((float)(startTimestamp - timestamp));
        StartCoroutine(StartOpenCall(startTimestamp));
    }
    
    private IEnumerator StartOpenCall(double startTimestamp){
        double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
        yield return new WaitForSeconds((float)(startTimestamp - timestamp));
        openingClipAudioSource.volume = 1f;
        yield return new WaitForSeconds(openingClipLength);
        openingClipAudioSource.volume = 0f;
        audioSources[defaultClipIndex].volume = 1f;
        openingMusicPlayed = true;
    }
    
    void StopMusicLoops() {
        crossFadeQueue.Clear();
        isCrossFadeRunning = false;
        baseClipAudioSource.Stop();
        baseClipAudioSource.volume = 0f;
        foreach (AudioSource audioSource in audioSources) {
            audioSource.Stop();
            audioSource.volume = 0f;
        }
        openingClipAudioSource.Stop();
        openingClipAudioSource.volume = 0f;
        openingMusicPlayed = false;
    }

    public void IncrementCount(bool isHero, TeamID teamID){
        switch(teamID){
            case TeamID.blue:
                if(isHero) cowboyHeroCount++;
                else       cowboyGruntCount++;
                break;
            case TeamID.red:
                if(isHero) vikingHeroCount++;
                else       vikingGruntCount++;
                break;
        }
    }
    public void DecrementCount(bool isHero, TeamID teamID){
        switch(teamID){
            case TeamID.blue:
                if(isHero) cowboyHeroCount--;
                else       cowboyGruntCount--;
                break;
            case TeamID.red:
                if(isHero) vikingHeroCount--;
                else       vikingGruntCount--;
                break;
        }
    }
    
}
