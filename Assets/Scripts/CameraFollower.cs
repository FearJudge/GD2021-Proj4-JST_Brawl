using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollower : MonoBehaviour
{
    public Vector3 move;
    public Transform target;
    public float camSpeedX;
    public float camSpeedY;
    public Vector3 camOffSet;
    public DepthBeUController controls;

    public Transform setSights;
    public static bool freeCam = false;
    bool disableFreeCam = false;
    public static bool camDepthChange = false;
    public static Vector3 setCamRotation = Vector3.zero;
    [Range(0f, 100f)] public float freecamSpeed = 1f;
    public PlayerInput pi;

    public void Awake()
    {
        setSights = target;
        if (setCamRotation != Vector3.zero)
        {
            Camera.main.transform.rotation = Quaternion.Euler(setCamRotation);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (freeCam) { FreeCamMode(); }
        move = transform.position;
        move.x = Mathf.Lerp(move.x, setSights.position.x+camOffSet.x, camSpeedX * Time.deltaTime);
        if (!controls.airborne)
        {
            move.y = Mathf.Lerp(move.y, setSights.position.y + camOffSet.y, camSpeedY * Time.deltaTime);
        }
        move.z = camOffSet.z;
        transform.position = move;
    }

    public void FreeCamMode()
    {
        if (disableFreeCam) { return; }
        if (pi.actions.FindAction("Jump").triggered) { disableFreeCam = true; }
        Vector2 dpth = pi.actions.FindAction("CameraPan").ReadValue<Vector2>();
        freecamSpeed += dpth.x * 0.2f;
        if (freecamSpeed < 0f) { freecamSpeed = 0f; } else if (freecamSpeed > 100f) { freecamSpeed = 100f; }
        camOffSet += new Vector3(pi.actions.FindAction("Move").ReadValue<Vector2>().x * Time.deltaTime * freecamSpeed, pi.actions.FindAction("Move").ReadValue<Vector2>().y * Time.deltaTime * freecamSpeed, dpth.y * Time.deltaTime * freecamSpeed);
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
