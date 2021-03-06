﻿using UnityEngine;

public class CameraPosition : MonoBehaviour {
    public float speed;
    private Vector3 initialPositionLeft = new Vector3(50f,32f,277.5f);
    private Vector3 rotationLeft = new Vector3(40f, 180f, 0f);
    private Vector3 initialPositionRight = new Vector3(50f,32f,22.5f);
    private Vector3 rotationRight = new Vector3(40.0f, 0f, 0f);

    private bool isServer;
    
    private ComputerLane currentLane;

    // Use this for initialization
    void Start () {
        currentLane = GraniteNetworkManager.lane;
        int screenNumber = GraniteNetworkManager.screeNumber;
        isServer = GraniteNetworkManager.isServer;
        Debug.Log("Screen Number: " + screenNumber);
        
        float width = 100; //the width of the screen in the game 
        Vector3 v3 = currentLane == ComputerLane.LEFT ? initialPositionLeft : initialPositionRight; //get current pos
        v3.x = width / 2 + width*screenNumber; //offset the camera correctly
        transform.position = v3;
        print(transform.rotation.eulerAngles);
        transform.rotation = Quaternion.Euler(currentLane == ComputerLane.LEFT ? rotationLeft : rotationRight);
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
            if(Input.GetKeyDown(KeyCode.V)){
                //switch view to other lane
                transform.rotation = Quaternion.Euler(currentLane == ComputerLane.RIGHT ? rotationLeft : rotationRight);
                Vector3 v3 = transform.position;
                v3.z = currentLane == ComputerLane.RIGHT ? initialPositionLeft.z : initialPositionRight.z;
                transform.position = v3;
                currentLane = currentLane == ComputerLane.RIGHT ? ComputerLane.LEFT : ComputerLane.RIGHT;
            }
        }

    }
}
