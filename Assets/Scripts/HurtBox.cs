using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public float hitStun = 0.25f;
    public int damage = 10;
    public bool knockDown = false;
    public Vector3 knockDownVelocity = Vector3.zero;
    public LayerMask activeOn;
}
