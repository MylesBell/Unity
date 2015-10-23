using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MotionSynch : NetworkBehaviour {

    [SyncVar] private Vector3 synchPos;
    [SyncVar] private float synchYRot;

    private Vector3 lastPos;
    private Quaternion lastRot;
    private Transform currentTransform;
    public float lerpRate = 10f;
    public float positionThreshold = 0.5f;
    public float rotationThreshold = 5f;
	// Use this for initialization
	void Start () {
        currentTransform = transform;
	}
	
	// Update is called once per frame
	void Update () {
        TransmitMotion();
        LerpMotion();
	}

    void TransmitMotion()
    {
        if (isServer)
        {
            if(Vector3.Distance(currentTransform.position,lastPos) > positionThreshold
                || Quaternion.Angle(currentTransform.rotation,lastRot) > rotationThreshold)
            {
                lastPos = currentTransform.position;
                lastRot = currentTransform.rotation;

                synchPos = currentTransform.position;
                synchYRot = currentTransform.localEulerAngles.y;
            }
        }
    }

    void LerpMotion()
    {
        if (!isServer)
        {
            currentTransform.position = Vector3.Lerp(currentTransform.position, synchPos, Time.deltaTime * lerpRate);
            currentTransform.rotation = Quaternion.Lerp(currentTransform.rotation, Quaternion.Euler(new Vector3(0, synchYRot, 0)), Time.deltaTime*lerpRate);
        }
    }
}
