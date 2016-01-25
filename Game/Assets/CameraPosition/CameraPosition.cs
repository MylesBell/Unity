using UnityEngine;

public class CameraPosition : MonoBehaviour {
    public float speed;
    private Vector3 initialPositionLeft = new Vector3(50f,32f,377.5f);
    private Vector3 rotationLeft = new Vector3(40f, 180f, 0f);
    private Vector3 initialPositionRight = new Vector3(50f,32f,17.5f);
    private Vector3 rotationRight = new Vector3(40.0f, 0f, 0f);

    private bool isServer;
    
    private CreateTerrain.ComputerLane currentLane;

    // Use this for initialization
    void Start () {
        int numScreensLeft = PlayerPrefs.GetInt("numberofscreens-left", 0);
        int numScreensRight = PlayerPrefs.GetInt("numberofscreens-right", 0);
        
        int screenNumber = PlayerPrefs.GetInt("screen", 0);
        isServer = PlayerPrefs.GetInt("isServer", 1) == 1 ? true : false;
        Debug.Log("Screen Number: " + screenNumber);
        
        float width = 100; //the width of the screen in the game 
        Vector3 v3 = numScreensLeft > 0 ? initialPositionLeft : initialPositionRight; //get current pos
        v3.x = width / 2 + width*screenNumber; //offset the camera correctly
        transform.position = v3;
        Quaternion rotation = Quaternion.identity;
        print(transform.rotation.eulerAngles);
        transform.rotation = Quaternion.Euler(numScreensLeft > 0 ? rotationLeft : rotationRight);
        currentLane = numScreensLeft > 0 ? CreateTerrain.ComputerLane.LEFT : CreateTerrain.ComputerLane.RIGHT;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isServer)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
            }
            if(Input.GetKey(KeyCode.V)){
                //switch view to other lane
                transform.rotation = Quaternion.Euler(currentLane == CreateTerrain.ComputerLane.RIGHT ? rotationLeft : rotationRight);
                Vector3 v3 = transform.position;
                v3.z = currentLane == CreateTerrain.ComputerLane.RIGHT ? initialPositionLeft.z : initialPositionRight.z;
                transform.position = v3;
                currentLane = currentLane == CreateTerrain.ComputerLane.RIGHT ? CreateTerrain.ComputerLane.LEFT : CreateTerrain.ComputerLane.RIGHT;
            }
        }

    }
}
