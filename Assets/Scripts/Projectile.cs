using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float lifetime;

    public void BeginDeath()
    {
        Invoke("Die", lifetime);
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
