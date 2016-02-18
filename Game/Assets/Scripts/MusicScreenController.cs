using System.Collections;
using UnityEngine;

public class MusicScreenController : MonoBehaviour {
    public AudioClip[] MusicClips;
    public float crossFadeDuration;
    // public AudioClip[] MusicClipForCowboys;

    private int redHeroCount;
    private int redGruntCount;
    private int blueHeroCount;
    private int blueGruntCount;
    
    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private AudioClip defaultClip;
    private AudioClip playingClip;
    private bool usingSource1 = true;
    private bool isCrossFadeRunning = false;
    private Coroutine CrossFadeCoroutine;
    
	void Start () {
        redHeroCount = 0;
        redGruntCount = 0;
        blueHeroCount = 0;
        blueGruntCount = 0;
        gameObject.transform.position = Camera.main.transform.position;
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource1.loop = true;
        audioSource1.playOnAwake = false;
        audioSource1.Stop();
        audioSource2 = gameObject.AddComponent<AudioSource>();
        audioSource2.loop = true;
        audioSource2.playOnAwake = false;
        audioSource2.Stop();
        //Initially some screens will not have units on them, so play default music depending on screen position
        int numScreensLeft = PlayerPrefs.GetInt("numberofscreens-left", 0);
        int numScreensRight = PlayerPrefs.GetInt("numberofscreens-right", 0);
        int screenNumber = PlayerPrefs.GetInt("screen", 0);
        int numScreens = PlayerPrefs.GetInt("lane", 0) == 0 ? numScreensLeft : numScreensRight;
        defaultClip = screenNumber < numScreens / 2 ? MusicClips[0] : MusicClips[MusicClips.Length - 1];
        playingClip = defaultClip;
        playClip(playingClip);
        audioSource1.volume = 1;
        audioSource2.volume = 0;
	}
    
	void Update () {
        int redCount = redGruntCount + redHeroCount;
        int blueCount = blueGruntCount + blueHeroCount;
        // Debug.Log("Reds " + redCount + " blues " + blueCount);
        AudioClip newClip;
        if(redCount == 0 && blueCount == 0) newClip = playingClip; //neither of them present, hence keep playing tunes
        else if(redCount == 0) newClip = MusicClips[0]; //only blue present, play their tune
        else if(blueCount == 0) newClip = MusicClips[MusicClips.Length - 1]; //only red present, play their tune
        else if(redCount == blueCount) newClip = MusicClips[MusicClips.Length/2]; //even number of each, play "equal" tunes
        else {
             //agree about number of tracks
             //then create ranges for 1/abs(redCount-blueCount) which point to a track taking the diff into account
             if(redCount > blueCount) newClip = MusicClips[MusicClips.Length - 1];
             else                     newClip = MusicClips[0];
        }
        if(newClip != playingClip){
            //could be smoother for transition between music
            if(isCrossFadeRunning) StopCoroutine(CrossFadeCoroutine);
            CrossFadeCoroutine = StartCoroutine(CrossFade(crossFadeDuration, newClip));
            playingClip = newClip;
        }
	}
    
    private IEnumerator CrossFade(float duration, AudioClip newClip){
        isCrossFadeRunning = true;
        float fTimeCounter = 0f;
        AudioSource currentAudioSource = usingSource1 ? audioSource1 : audioSource2;
        AudioSource newAudioSource = usingSource1 ? audioSource2 : audioSource1;
        newAudioSource.clip = newClip;
        newAudioSource.Play();
        usingSource1 = !usingSource1;
        while (!(Mathf.Approximately(fTimeCounter, duration))) {
            fTimeCounter = Mathf.Clamp01(fTimeCounter + Time.deltaTime);
            currentAudioSource.volume = 1f - fTimeCounter;
            newAudioSource.volume = fTimeCounter;
            yield return new WaitForSeconds(0.02f);
        }
        isCrossFadeRunning = false;
    }
    
    private void playClip(AudioClip clip){
        AudioSource aSource = usingSource1 ? audioSource1 : audioSource2;
        aSource.clip = playingClip;
        aSource.Play();
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
