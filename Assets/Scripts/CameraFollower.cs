using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Vector2 move;
    public Transform target;
    public float camSpeedX;
    public float camSpeedY;
    public Vector2 camOffSet;
    public DepthBeUController controls;

    public Transform setSights;

    public void Awake()
    {
        setSights = target;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        move = transform.position;
        move.x = Mathf.Lerp(move.x, setSights.position.x+camOffSet.x, camSpeedX * Time.deltaTime);
        if (!controls.airborne)
        {
            move.y = Mathf.Lerp(move.y, setSights.position.y + camOffSet.y, camSpeedY * Time.deltaTime);
        }
        transform.position = move;
    }

    public void SetEncounterCamera(Vector3 spot)
    {
        GameObject camTarget = new GameObject("EncounterFollowerTarget");
        camTarget.transform.position = spot;
        setSights = camTarget.transform;
    }

    public void EndEncounterCamera()
    {
        setSights = target;
    }
}
