using UnityEngine;
using System.Collections;

public class Navigator : MonoBehaviour {
    
    public enum State {Calculating, Moving, Idle}
    
    public Transform target; //For testing
    public float speed = 10;
    State state = State.Idle;
    bool reachedTarget = false;
    Vector3 targetPosition;
    
    LongPathGrid longPathGrid;
    Vector3[] path;
    int targetIndex;
    bool initialised = false;
     
    public void InitialiseNavigator(LongPathGrid longGrid) {
        if (!initialised) {
            this.longPathGrid = longGrid;
            this.targetPosition = target.position; //For testing
            initialised = true;
        }
    }
    
    public void SetNewTarget(Vector3 target) {
        state = State.Idle;
        reachedTarget = false;
        targetPosition = target;
    }
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.N))
            transform.position = new Vector3(Random.Range(0,200),0,Random.Range(0,100));
        if (Input.GetKeyDown(KeyCode.M)) { //For testing
            this.targetPosition = target.position;
            reachedTarget = false;
        }
        if (initialised && state == State.Idle && !reachedTarget) {
            state = State.Calculating;          
            TryNavigate();
        }
    }
    
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
        if (pathSuccessful && newPath.Length > 0) {
            path = newPath;
            state = State.Moving;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        } else {
            state = State.Idle;
        }
    }
    
    IEnumerator FollowPath() {
        Vector3 currentWaypoint = path[0];
        targetIndex = 0;
        reachedTarget = false;
		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex ++;
				if (targetIndex >= path.Length) {
                    reachedTarget = true;
                    state = State.Idle;
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}

			transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			yield return null;
		}
    }
    
    void TryNavigate() {
        NavGridManager.RequestLongPath(this.transform.position, targetPosition, longPathGrid, OnPathFound);
    }
    
    void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}
}