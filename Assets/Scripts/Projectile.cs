using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float lifetime;
    [SerializeField] Rigidbody rb;
    [SerializeField] ParticleSystem ps;
    public Vector3 frwrd;
    public bool rotate = true;
    public float killAfter = -1f;
    public float stopAfter = 0f;

    public void Awake()
    {
        if (killAfter > 0f) { lifetime = killAfter; BeginDeath(); }
    }

    public void Update()
    {
        if (!rotate) { return; }
        transform.rotation = Quaternion.LookRotation(rb.velocity, frwrd);
    }

    public void BeginDeath()
    {
        if (ps != null) { Invoke("Close", stopAfter); }
        Invoke("Die", lifetime);
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void Close()
    {
        ps.Stop();
    }
}
