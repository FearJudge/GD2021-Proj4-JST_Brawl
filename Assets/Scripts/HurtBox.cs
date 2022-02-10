using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public float hitStun = 0.25f;
    public int damage = 10;
    public bool knockDown = false;
    public bool parry = false;
    public Vector3 knockDownVelocity = Vector3.zero;
    public LayerMask activeOn;

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter!");
        if ((1 << other.gameObject.layer & activeOn) != 0)
        {
            other.transform.parent.gameObject.TryGetComponent(out DepthBeUController enemy);
            Debug.Log(enemy.name);
            if (enemy == null) { return; }
            bool dirIsLeft = false;
            if (other.transform.position.x - transform.position.x > 0) { dirIsLeft = true; }
            enemy.GetHit(damage, hitStun, knockDown, knockDownVelocity, dirIsLeft);
        }
    }
}
