using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hero : NetworkBehaviour, IHeroMovement {
    [SyncVar] public TeamID teamID;
    [SyncVar] public float speed = 3.0f;
    [SyncVar] public float health = 10.0f;
	private NavMeshAgent agent;
    [SyncVar] public Vector3 tempTargetLocation;
    [SyncVar] public bool isInitialised = false;

    [SyncVar] private Vector3 synchPos;
    [SyncVar] private float synchYRot;

    private Vector3 lastPos;
    private Quaternion lastRot;
    public float lerpRate = 10f;
    public float positionThreshold = 0.5f;
    public float rotationThreshold = 5f;

    void Start() {
        synchPos = transform.position;
    }
    void Update () {
        if (isInitialised)
        {
            if (isServer)
            {
                transform.position = Vector3.Lerp(transform.position, this.tempTargetLocation, Time.deltaTime);
                if (Vector3.Distance(transform.position, lastPos) > positionThreshold
                    || Quaternion.Angle(transform.rotation, lastRot) > rotationThreshold)
                {
                    lastPos = transform.position;
                    lastRot = transform.rotation;

                    synchPos = transform.position;
                    synchYRot = transform.localEulerAngles.y;
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, synchPos, Time.deltaTime * lerpRate);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, synchYRot, 0)), Time.deltaTime * lerpRate);
            }
        }
    }

	public void InitialiseHero(TeamID teamID, Vector3 targetPosition) {
		SetTeam (teamID);
		SetTargetPosition (targetPosition);
        isInitialised = true;
    }

	void SetTeam(TeamID id) {
		this.teamID = id;
		//Material mat = GetComponent<Renderer> ().material;
		//Light light = transform.GetComponentInChildren<Light> ();
		//if (id == TeamID.red) {
		//	mat.SetColor ("_Color", Color.red);
		//	mat.SetColor("_EmissionColor", Color.red);
		//	light.color = Color.red;
		//} else {
		//	mat.SetColor ("_Color", Color.blue);
		//	mat.SetColor("_EmissionColor", Color.blue);
		//	light.color = Color.blue;
			
		//}
	}

	void SetTargetPosition (Vector3 targetPosition) {
		//Temporary until NavMesh generation is implemented in terrain data
		this.tempTargetLocation = targetPosition;
		NavMeshAgent agent = GetComponent<NavMeshAgent> ();
		//		agent.destination = targetPosition;
	}
	
	#region IHeroMovement implementation
	public void PlayerBack ()
	{
		throw new System.NotImplementedException ();
	}
	public void PlayerMoveLane (Direction direction)
	{
		throw new System.NotImplementedException ();
	}
	#endregion
}
