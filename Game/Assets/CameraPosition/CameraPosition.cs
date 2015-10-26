using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CameraPosition : MonoBehaviour {
    public float speed;

    private bool isServer;

    // Use this for initialization
    void Start () {
        int screenNumber = PlayerPrefs.GetInt("screen", 0);
        isServer = PlayerPrefs.GetInt("isServer", 1) == 1 ? true : false;
        Debug.Log("Screen Number: " + screenNumber);
        
        float width = 100; //the width of the screen in the game 
        Vector3 v3 = transform.position; //get current pos
        v3.x = width / 2 + width*screenNumber; //offset the camera correctly
        transform.position = v3;

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
        }

    }
}
