using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedControllerForEnemies : MonoBehaviour
{
    public DepthBeUController controller;
    public float speedmod = 1f;
    public bool invulnerable = false;
    bool previnv = false;
    float prevspeedmod = -0.75f;
    float defspeed = 4f;

    public void Start()
    {
        defspeed = controller.speed;
    }

    public void Update()
    {
        if (prevspeedmod != speedmod) { controller.speed = defspeed * speedmod; prevspeedmod = speedmod; }
        if (invulnerable != previnv) { controller.Invulnerable = invulnerable; previnv = invulnerable; }
    }
}
