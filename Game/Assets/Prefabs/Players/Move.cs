using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{

    Vector3 pointB;
    Vector3 pointA;
    // Use this for initialization
    IEnumerator Start()
    {
        int numScreens = PlayerPrefs.GetInt("numberofscreens", 2);
        pointB = new Vector3(numScreens*100-50, 10, 50);
        pointA = transform.position;
        while (true)
        {
            yield return StartCoroutine(MoveObject(transform, pointA, pointB, 10.0f));
            yield return StartCoroutine(MoveObject(transform, pointB, pointA, 10.0f));
        }
    }


    IEnumerator MoveObject(Transform thisTransform, Vector3 startPos, Vector3 endPos, float time)
    {
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            thisTransform.position = Vector3.Lerp(startPos, endPos, i);
            yield return null;
        }
    }
}
