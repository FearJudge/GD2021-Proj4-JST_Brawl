using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    [SerializeField] private InputStreamParser inputSystem;

    public Health ownerHealth;
    public Special ownerSpecial;
    public Transform body;
    public float hitStun = 0.25f;
    public int damage = 10;
    public int lifeSteal = 0;
    public int crit = 0;
    public int bloodSteal = 0;
    public float resistanceMod = 0f;
    public bool knockDown = false;
    public bool parry = false;
    public Vector3 knockDownVelocity = Vector3.zero;
    public LayerMask activeOn;
    [HideInInspector] public bool useAlternate;
    [HideInInspector] public LayerMask activeOnAlt;
    public GameObject blockSpark;
    public GameObject hitSpark;
    public GameObject critSpark;
    public GameObject[] sparkVariants;
    bool isCrit = false;

    public void OnTriggerEnter(Collider other)
    {
        if (!useAlternate) { NormalHurtBox(other); }
        else { ParryHurtBox(other); }
    }

    public void NormalHurtBox(Collider other)
    {
        if ((1 << other.gameObject.layer & activeOn) != 0)
        {
            DepthBeUController enemy = other.gameObject.GetComponentInParent<DepthBeUController>();
            if (enemy == null) { return; }
            if (!enemy.isAlive) { return; }
            if (blockSpark != null && (enemy.Invulnerable || enemy.resistance <= 0f))
            { Instantiate(blockSpark, other.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0), transform.rotation); return; }
            if (ownerHealth != null) { ownerHealth.Hp += lifeSteal; }
            if (ownerSpecial != null) { ownerSpecial.Value += bloodSteal; }
            bool dirIsLeft = false;
            if (body != null) { if (body.localRotation.y == 0f) { dirIsLeft = true; } }
            isCrit = (Random.Range(0, 101) < crit);
            if (isCrit) { damage *= 2; lifeSteal *= 2; hitStun *= 1.5f; }
            enemy.GetHit(damage, hitStun, knockDown, knockDownVelocity, dirIsLeft, isCrit, resistanceMod + 1f);
            if (hitSpark != null) { Instantiate(hitSpark, other.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0), transform.rotation); }
            if (critSpark != null && isCrit) { Instantiate(critSpark, other.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0), transform.rotation); }
            if (inputSystem != null) { inputSystem.HitConfirmed(); }
        }
    }

    public void ParryHurtBox(Collider other)
    {
        Debug.Log(activeOnAlt.value);
        Debug.Log(other.gameObject.name);
        if ((1 << other.gameObject.layer & activeOnAlt) != 0)
        {
            Debug.Log("Noice!");
            if (blockSpark != null)
            { Instantiate(blockSpark, other.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0), transform.rotation); }
            if (ownerHealth != null) { ownerHealth.Hp += lifeSteal; }
            if (inputSystem != null) { inputSystem.HitConfirmed(); }
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
        sparkVariants = copy.sparkVariants;
        isCrit = copy.isCrit;
    }
}
