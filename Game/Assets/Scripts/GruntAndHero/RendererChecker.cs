using UnityEngine;

public class RendererChecker : MonoBehaviour {
    private TeamID teamID;
    private MusicScreenController musicScreenController;
    
    private bool visible;

    void Start() {
        musicScreenController = GameObject.FindGameObjectsWithTag("musicPlayer")[0].GetComponent<MusicScreenController>();
        teamID = gameObject.tag.Contains("red") ? TeamID.red : TeamID.blue;
        if(visible = isVisible()) IncrementCount();
    }
    
    void Update(){
        if(visible != isVisible()){
            visible = !visible;
            if(visible) IncrementCount();
            else        DecrementCount();
        }
    }
    
    private bool isVisible(){
        Vector3 v3 = Camera.main.WorldToViewportPoint(transform.position);
        return Mathf.Clamp01(v3.x)==v3.x && Mathf.Clamp01(v3.y)==v3.y && v3.z > 0.0f;
    }
    
    void IncrementCount(){
        musicScreenController.IncrementCount(false, teamID);
    }
    void DecrementCount(){
        musicScreenController.DecrementCount(false, teamID);
    }
    
    void OnDisable(){
        if(visible) DecrementCount();
    }
    // void OnBecameVisible(){
    //     musicScreenController.IncrementCount(false, teamID);
    // }
    // void OnBecameInvisible(){
    //     musicScreenController.DecrementCount(false, teamID);
    // }
}