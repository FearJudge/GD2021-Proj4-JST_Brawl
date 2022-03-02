using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public Health ownerHealth;
    public Transform body;
    public float hitStun = 0.25f;
    public int damage = 10;
    public int lifeSteal = 0;
    public int crit = 0;
    public bool knockDown = false;
    public bool parry = false;
    public Vector3 knockDownVelocity = Vector3.zero;
    public LayerMask activeOn;
    public GameObject hitSpark;
    public GameObject critSpark;
    public GameObject[] sparkVariants;
    bool isCrit = false;

    public void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & activeOn) != 0)
        {
            other.transform.parent.gameObject.TryGetComponent(out DepthBeUController enemy);
            if (enemy == null) { return; }
            if (ownerHealth != null) { ownerHealth.Hp += lifeSteal; }
            bool dirIsLeft = false;
            if (body.localRotation.y == 0f) { dirIsLeft = true; }
            isCrit = (Random.Range(0, 101) < crit);
            enemy.GetHit(damage, hitStun, knockDown, knockDownVelocity, dirIsLeft, isCrit);
            if (hitSpark != null) { Instantiate(hitSpark, other.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0), transform.rotation); }
            if (critSpark != null && isCrit) { Instantiate(critSpark, other.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0), transform.rotation); }
        }
    }

    public void SetData(HurtBox copy)
    {
        ownerHealth = copy.ownerHealth;
        body = copy.body;
        hitStun = copy.hitStun;
        damage = copy.damage;
        lifeSteal = copy.lifeSteal;
        crit = copy.crit;
        knockDown = copy.knockDown;
        knockDownVelocity = copy.knockDownVelocity;
        activeOn = copy.activeOn;
        hitSpark = copy.hitSpark;
        critSpark = copy.critSpark;
        sparkVariants = copy.sparkVariants;
        isCrit = copy.isCrit;
    }
}
