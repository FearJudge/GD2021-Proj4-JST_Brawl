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


    // Update is called once per frame
    void LateUpdate()
    {
        move = transform.position;
        move.x = Mathf.Lerp(move.x, target.position.x+camOffSet.x, camSpeedX * Time.deltaTime);
        move.y = Mathf.Lerp(move.y, target.position.y+camOffSet.y, camSpeedY * Time.deltaTime);
        transform.position = move;
    }
}
