using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public Transform body;
    public float hitStun = 0.25f;
    public int damage = 10;
    public bool knockDown = false;
    public bool parry = false;
    public Vector3 knockDownVelocity = Vector3.zero;
    public LayerMask activeOn;

    public void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & activeOn) != 0)
        {
            other.transform.parent.gameObject.TryGetComponent(out DepthBeUController enemy);
            if (enemy == null) { return; }
            bool dirIsLeft = false;
            if (body.localRotation.y == 0f) { dirIsLeft = true; }
            enemy.GetHit(damage, hitStun, knockDown, knockDownVelocity, dirIsLeft);
        }
    }
}
