using UnityEngine;

public class MusicScreenController : MonoBehaviour {

    private int redHeroCount;
    private int redGruntCount;
    private int blueHeroCount;
    private int blueGruntCount;
    
	void Start () {
	   redHeroCount = 0;
       redGruntCount = 0;
       blueHeroCount = 0;
       blueGruntCount = 0;
	}
    
	void Update () {
	   Debug.Log("Red:\tHeros:"+redHeroCount+"\tGrunts:" +redGruntCount +"\n"
                +"Blue:\tHeros:"+blueHeroCount+"\tGrunts:" +blueGruntCount);
	}
    
    public void IncrementRedTeam(bool isHero){
        if(isHero) redHeroCount++;
        else       redGruntCount++;
    }
    public void DecrementRedTeam(bool isHero){
        if(isHero) redHeroCount--;
        else       redGruntCount--;
    }
    
    public void IncrementBlueTeam(bool isHero){
        if(isHero) blueHeroCount++;
        else       blueGruntCount++;
    }
    public void DecrementBlueTeam(bool isHero){
        if(isHero) blueHeroCount--;
        else       blueGruntCount--;
    }
    
}
