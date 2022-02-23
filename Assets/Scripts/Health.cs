using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    protected DepthBeUController character;
    [SerializeField] public string healthOwnerName = "";
    public bool isPlayer = false;
    private int maxhealth = 100;
    public double lastTouched = 0f;
    [SerializeField] private int health = 25;
    public int Hp
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            lastTouched = Time.fixedTimeAsDouble;
            UI_HealthBarBrain.NotifyBrain(this, healthOwnerName, isPlayer);
            if (health < 0) { health = 0; }
            if (health == 0 && !isPlayer)
            {
                UI_HealthBarBrain.NotifyBrainOfDeath(this, isPlayer);
                character.Kill();
            }
        }
    }

    void Start()
    {
        character = gameObject.GetComponent<DepthBeUController>();
        maxhealth = health;
        if (isPlayer) { UI_HealthBarBrain.NotifyBrain(this, healthOwnerName, isPlayer); }
    }

    public int GetMaxHealth()
    {
        return maxhealth;
    }

    public void AddMaxHealth(int value)
    {
        maxhealth += value;
    }

    public void ModMaxHealth(float value)
    {
        maxhealth = (int)(maxhealth * value);
    }

    public void FullRecover()
    {
        Hp = maxhealth;
    }
}