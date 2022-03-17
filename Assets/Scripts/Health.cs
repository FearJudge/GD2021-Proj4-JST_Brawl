using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public delegate void PlayerHealthEvent();
    public delegate void CharacterHealthEvent(DepthBeUController character);
    public static event PlayerHealthEvent GotHit;
    public static event CharacterHealthEvent ThresholdReached;

    [SerializeField] protected DepthBeUController character;
    [SerializeField] public string healthOwnerName = "";
    public int iconid = 0;
    public bool isPlayer = false;
    private int maxhealth = 100;
    private int minhealthInd = 0;
    public double lastTouched = 0f;
    public int[] healthThresholds = new int[0];
    [SerializeField] private int health = 25;
    public int Hp
    {
        get
        {
            return health;
        }
        set
        {
            int last = health;
            health = value;
            if (last > health && isPlayer) { GotHit?.Invoke(); }
            lastTouched = Time.fixedTimeAsDouble;
            UI_HealthBarBrain.NotifyBrain(this, healthOwnerName, isPlayer);
            if (health > maxhealth) { health = maxhealth; }
            if (minhealthInd >= 0) { if (health < healthThresholds[minhealthInd]) { NextThreshold(); return; } }
            if (health < 0) { health = 0; }
            if (health == 0 && !isPlayer)
            {
                UI_HealthBarBrain.NotifyBrainOfDeath(this, isPlayer);
                character.Kill();
            }
            if (health == 0 && isPlayer)
            {
                character.Kill();
            }
        }
    }

    void Start()
    {
        character = gameObject.GetComponent<DepthBeUController>();
        maxhealth = health;
        minhealthInd = healthThresholds.Length - 1;
        if (isPlayer) { healthOwnerName = GameOptions.playerName; UI_HealthBarBrain.NotifyBrain(this, healthOwnerName, isPlayer); }
    }

    void NextThreshold()
    {
        health = healthThresholds[minhealthInd];
        ThresholdReached?.Invoke(character);
        character.Resistance(999f);
        minhealthInd--;
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