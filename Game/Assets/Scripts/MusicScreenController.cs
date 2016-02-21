using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MusicScreenController : NetworkBehaviour {
    
    public AudioClip baseClip;
    public AudioClip[] MusicClips;
    public float crossFadeDuration;
    
    public float musicStartSecondsOffset;
    // public AudioClip[] MusicClipForCowboys;

    private int redHeroCount;
    private int redGruntCount;
    private int blueHeroCount;
    private int blueGruntCount;
    
    private AudioSource baseClipAudioSource;
    private AudioSource[] audioSources;
    private int defaultClipIndex;
    private int playingClipIndex;
    private bool isCrossFadeRunning = false;
    private Coroutine CrossFadeCoroutine;
    
    private bool musicStarted = false;
    
	void Start () {
        if(MusicClips.Length == 0) enabled = false;
        else {
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
            baseClipAudioSource.volume = 1f;
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
            int numScreensLeft = PlayerPrefs.GetInt("numberofscreens-left", 0);
            int numScreensRight = PlayerPrefs.GetInt("numberofscreens-right", 0);
            int screenNumber = PlayerPrefs.GetInt("screen", 0);
            int numScreens = PlayerPrefs.GetInt("lane", 0) == 0 ? numScreensLeft : numScreensRight;
            defaultClipIndex = screenNumber < numScreens / 2 ? 0 : MusicClips.Length - 1;
            playingClipIndex = defaultClipIndex;
            audioSources[defaultClipIndex].volume = 1f;
        }
	}
    
	void Update () {
        if(!musicStarted && Input.GetKeyDown(KeyCode.M)){
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 1, 0, 0, System.DateTimeKind.Utc);
            double startMusicTimestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds + musicStartSecondsOffset;
            RpcStartMusicLoops(startMusicTimestamp);
        }
        if(musicStarted) {
            int redCount = redGruntCount + redHeroCount;
            int blueCount = blueGruntCount + blueHeroCount;
            // Debug.Log("Reds " + redCount + " blues " + blueCount);
            int newClipIndex;
            if(redCount == 0 && blueCount == 0) newClipIndex = playingClipIndex; //neither of them present, hence keep playing tunes
            else if(redCount == 0) newClipIndex = 0; //only blue present, play their tune
            else if(blueCount == 0) newClipIndex = MusicClips.Length - 1; //only red present, play their tune
            else if(redCount == blueCount) newClipIndex = MusicClips.Length/2; //even number of each, play "equal" tunes
            else {
                //agree about number of tracks
                //then create ranges for 1/abs(redCount-blueCount) which point to a track taking the diff into account
                if(redCount > blueCount) newClipIndex = MusicClips.Length - 1;
                else                     newClipIndex = 0;
            }
            if(newClipIndex != playingClipIndex){
                CrossFadeCoroutine = StartCoroutine(CrossFade(audioSources[playingClipIndex], audioSources[newClipIndex], crossFadeDuration));
                playingClipIndex = newClipIndex;
            }
        }
	}
    
    private IEnumerator CrossFade(AudioSource currentAudioSource, AudioSource newAudioSource, float duration){
        isCrossFadeRunning = true;
        float fTimeCounter = 0f;
        while (!(Mathf.Approximately(fTimeCounter, duration))) {
            fTimeCounter = Mathf.Clamp01(fTimeCounter + Time.deltaTime);
            currentAudioSource.volume = 1f - fTimeCounter;
            newAudioSource.volume = fTimeCounter;
            yield return new WaitForSeconds(0.02f);
        }
        isCrossFadeRunning = false;
    }
    
    [ClientRpc]
    public void RpcStartMusicLoops(double startTimestamp) {
        StartMusicLoops(startTimestamp);
    }
    
    void StartMusicLoops(double startTimestamp) {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 1, 0, 0, System.DateTimeKind.Utc);
        double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
        baseClipAudioSource.PlayDelayed((float)(startTimestamp - timestamp));
        foreach (AudioSource audioSource in audioSources) {
            timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
            audioSource.PlayDelayed((float)(startTimestamp - timestamp));
        }
        musicStarted = true;
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
