using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum SpecialType { one, two, three };

[System.Serializable]
public struct SpecialFiles{
    public GameObject prefab;
    public Special special;
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
    
    // chosen identifiers
    public List<int> chosenNumbers;
    
    // required tags
    public string attackGruntTag;
    public string attackHeroTag;
    public string attackBaseTag;
    public string ownGruntTag;
    public string ownHeroTag;
    public string ownBaseTag;
    
    void Start() {
        // set required tags
        TargetSelect targetSelect = gameObject.GetComponent<TargetSelect>();
        attackGruntTag = targetSelect.attackGruntTag;
        attackHeroTag = targetSelect.attackHeroTag;
        attackBaseTag = targetSelect.attackBaseTag;
        ownGruntTag = targetSelect.teamID == TeamID.red ? "redGrunt" : "blueGrunt";
        ownHeroTag = targetSelect.teamID == TeamID.red ? "redHero" : "blueHero";
        ownBaseTag = targetSelect.teamID == TeamID.red ? "redBase" : "blueBase";
    }
    
    public void InitialiseSpecials(){
        // get number of powers
        int numberOfSpecials = specialFiles.Length;
        chosenNumbers = new List<int>();
        
        // choose three random powers and initialise them all
        int specialValue = getUniqueRandomInRange(numberOfSpecials, chosenNumbers);
        chosenNumbers.Add(specialValue);
        GameObject specialObject = (GameObject) Instantiate(specialFiles[specialValue].prefab, gameObject.transform.position,
            specialFiles[specialValue].prefab.transform.rotation);
        NetworkServer.Spawn(specialObject);
        specialOne = specialObject.GetComponent<Special>();
        RpcSetParent(specialObject,gameObject);
        
        // two
        specialValue = getUniqueRandomInRange(numberOfSpecials, chosenNumbers);
        chosenNumbers.Add(specialValue);
        specialObject = (GameObject) Instantiate(specialFiles[specialValue].prefab, gameObject.transform.position,
            specialFiles[specialValue].prefab.transform.rotation);
        NetworkServer.Spawn(specialObject);
        specialTwo = specialObject.GetComponent<Special>();
        RpcSetParent(specialObject,gameObject);
        
        // three
        specialValue = getUniqueRandomInRange(numberOfSpecials, chosenNumbers);
        chosenNumbers.Add(specialValue);
        specialObject = (GameObject) Instantiate(specialFiles[specialValue].prefab, gameObject.transform.position,
            specialFiles[specialValue].prefab.transform.rotation);
        NetworkServer.Spawn(specialObject);
        specialThree = specialObject.GetComponent<Special>();
        RpcSetParent(specialObject,gameObject);
        
        specialOne.transform.parent = gameObject.transform;
        specialTwo.transform.parent = gameObject.transform;
        specialThree.transform.parent = gameObject.transform;
        
        specialOne.InitialiseSpecial();
        specialTwo.InitialiseSpecial();
        specialThree.InitialiseSpecial();
    }
    
    private int getUniqueRandomInRange(int numberOfSpecials, List<int> chosenNumbers){
        int number;
        
        do{
            number = Random.Range(0, numberOfSpecials);
        }while (chosenNumbers.Contains(number));
        
        return number;
    }
    
    void Update() {
        if (Input.GetKeyUp(KeyCode.X)) {
            EmitSpecial(SpecialType.one);
        }
        if (Input.GetKeyUp(KeyCode.C)) {
            EmitSpecial(SpecialType.two);
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
        RpcPlayLevelUp();
        
        // upgrade all specials
        specialOne.UpgradeSpecial();
        specialTwo.UpgradeSpecial();
        specialThree.UpgradeSpecial();
    }
    
    [ClientRpc]
    
    public void RpcSetParent(GameObject child, GameObject parent) {
        child.transform.parent = parent.transform;
    }

    [ClientRpc]
    public void RpcPlayLevelUp() {
        GameObject levelUpParticle = (GameObject) Instantiate(LevelUpPrefab, gameObject.transform.position, LevelUpPrefab.transform.rotation);
        Destroy(levelUpParticle, levelUpParticle.GetComponent<ParticleSystem>().startLifetime);
    }
    
    public void ResetSpecials(){
        specialOne.ResetSpecial();
        specialTwo.ResetSpecial();
        specialThree.ResetSpecial();
    }
}