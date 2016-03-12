using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum SpecialType { one, two, three };

[System.Serializable]
public struct SpecialFiles{
    public GameObject prefab;
    public int identifier;
}

public class Specials : NetworkBehaviour, IPlayerSpecial {
    
    // prefabs  
    public GameObject LevelUpPrefab;
    public SpecialFiles[] specialFiles;
    
    // instantiated specials
    private Special specialOne;
    private Special specialTwo;
    private Special specialThree;
    private Special specialLevelUp;
    
    // chosen identifiers
    public List<int> chosenNumbers;
    
    // required tags
    public string attackGruntTag;
    public string attackHeroTag;
    public string attackBaseTag;
    public string ownGruntTag;
    public string ownHeroTag;
    public string ownBaseTag;
    
    public TeamID teamID;
    
    void Start() {
        // set required tags
        TargetSelect targetSelect = gameObject.GetComponent<TargetSelect>();
        attackGruntTag = targetSelect.attackGruntTag;
        attackHeroTag = targetSelect.attackHeroTag;
        attackBaseTag = targetSelect.attackBaseTag;
        
        teamID = targetSelect.teamID;
        ownGruntTag = teamID == TeamID.red ? "redGrunt" : "blueGrunt";
        ownHeroTag = teamID == TeamID.red ? "redHero" : "blueHero";
        ownBaseTag = teamID == TeamID.red ? "redBase" : "blueBase";
    }
    
    public void InitialiseSpecials(){
        // get number of powers
        int numberOfSpecials = specialFiles.Length;
        chosenNumbers = new List<int>();
        
         // choose three random powers and initialise them all
        specialOne = createSpecialPower(numberOfSpecials);
        specialTwo = createSpecialPower(numberOfSpecials);
        specialThree = createSpecialPower(numberOfSpecials);
        
        // initialise level up
        specialLevelUp = createSpecial(LevelUpPrefab);
    }
    
    private Special createSpecialPower(int numberOfSpecials){
        int specialValue = getUniqueRandomInRange(numberOfSpecials, chosenNumbers);
        chosenNumbers.Add(specialValue);
        return createSpecial(specialFiles[specialValue].prefab);
    }
    
    private Special createSpecial(GameObject prefab){
        GameObject specialObject = (GameObject) Instantiate(prefab, gameObject.transform.position,
            prefab.transform.rotation);
        NetworkServer.Spawn(specialObject);
        RpcSetParent(specialObject,gameObject);
        
        Special special = specialObject.GetComponent<Special>();
        special.transform.parent = gameObject.transform;
        special.InitialiseSpecial();
        
        return special;
    }
    
    private int getUniqueRandomInRange(int numberOfSpecials, List<int> chosenNumbers){
        int number = 5;
        
        do{
            // number = Random.Range(0, numberOfSpecials);
            number = number + 1;
        }while (chosenNumbers.Contains(number));
        
        return number;
    }
    
    void Update() {
        if (Input.GetKeyUp(KeyCode.Z)) {
            EmitSpecial(SpecialType.one);
        }
        if (Input.GetKeyUp(KeyCode.X)) {
            EmitSpecial(SpecialType.two);
        }
        if (Input.GetKeyUp(KeyCode.C)) {
            EmitSpecial(SpecialType.three);
        }
    }
    
    // implement IPlayerSpecial interface
    public void PlayerSpecial(SpecialType specialType)
    {
        EmitSpecial(specialType);
    }
    
    public void EmitSpecial(SpecialType specialType){
        switch(specialType){
            case SpecialType.one:
                specialOne.UseSpecial();
                break;
            case SpecialType.two:
                specialTwo.UseSpecial();
                break;
            default:
                specialThree.UseSpecial();
                break; 
        }
    }
    
    public void UpgradeSpecials(){
        // play upgrade animation
        specialLevelUp.UseSpecial();
        
        // upgrade all specials
        specialOne.UpgradeSpecial();
        specialTwo.UpgradeSpecial();
        specialThree.UpgradeSpecial();
    }
    
    [ClientRpc]
    public void RpcSetParent(GameObject child, GameObject parent) {
        child.transform.parent = parent.transform;
    }
    
    public void ResetSpecials(){
        specialOne.ResetSpecial();
        specialTwo.ResetSpecial();
        specialThree.ResetSpecial();
    }
}