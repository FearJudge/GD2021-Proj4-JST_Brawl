using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedControllerForEnemies : MonoBehaviour
{
    public DepthBeUController controller;
    public float speedmod = 1f;
    float prevspeedmod = 0f;
    float defspeed = 4f;

    public void Start()
    {
        defspeed = controller.speed;
    }

    public void Update()
    {
        if (prevspeedmod != speedmod) { controller.speed = defspeed * speedmod; prevspeedmod = speedmod; }
    }
}
