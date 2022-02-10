using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private int maxhealth = 100;
    [SerializeField] private int health = 25;
    [SerializeField] private IDestroyable root;
    [SerializeField] private bool isBoss = false;
    public int Hp
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            if (health < 0) { health = 0; }
            if (health == 0)
            {
            }
        }
    }

    void Start()
    {
        maxhealth = health;
    }
}

public interface IDestroyable
{
    void KillMe();
}