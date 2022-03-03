using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveProp
{
    public struct GlobalVariables
    {
        public int[] lifestealArray;
        public float[] hitstunArray;
        public int[] damagePlusArray;
        public float[] damageModArray;
        public int[] critArray;
    }

    [SerializeField] public readonly string moveName = "";
    public string animationTrigger = "";
    public AudioClip sound = null;
    public float animationSpeedMod = 1f;
    public int allowNextMove = 50;
    public int preventMovement = 10;
    public int followUpAllowFrom = 0;
    public Vector3 characterVelocity = Vector3.zero;
    public int hurtBoxDamage = 10;
    public int lifeSteal = 0;
    public int baseCrit = 0;
    public float hitStunDuration = 0.25f;
    public bool knockDown = false;
    public Vector3 knockDownVelocity = Vector3.zero;
    public bool isProjectile;
    public int ammoChange;
    public Vector3 projVelocity = Vector3.zero;
    public bool isProjBuff;

    public void ActivateMove(DepthBeUController player, GlobalVariables pv, int type)
    {
        if (player == null) { Debug.Log("No Controller!"); return; }
        if (sound != null) { SoundPlayer.PlaySFX(sound, 1f, 1f); }
        if (animationTrigger != "") { player.animator.SetTrigger(animationTrigger); }
        player.SetVelocity(characterVelocity, true);
        if (characterVelocity.y > 1f) { player.airborne = true; }
        player.hb.damage = Mathf.CeilToInt((hurtBoxDamage + pv.damagePlusArray[type]) * (1f + pv.damageModArray[type]));
        player.hb.hitStun = hitStunDuration * (1f + pv.hitstunArray[type]);
        player.hb.knockDown = knockDown;
        player.hb.knockDownVelocity = knockDownVelocity;
        player.hb.lifeSteal = lifeSteal + pv.lifestealArray[type];
        player.hb.crit = pv.critArray[type] + baseCrit;
        if (isProjectile) { player.projectilespawner.hbData = player.hb; player.projectilespawner.ammoChange = ammoChange; }
        if (isProjBuff)
        {
            player.projectilespawner.ammo += ammoChange;
            player.projectilespawner.extraVelocity = projVelocity;
            player.projectilespawner.extraDamage = hurtBoxDamage;
            player.projectilespawner.extraCrit = baseCrit;
            player.projectilespawner.extraStun = hitStunDuration;
        }
    }

    public void ActivateMove(DepthBeUController player)
    {
        ActivateMove(player, new GlobalVariables() { damageModArray = new float[1], hitstunArray = new float[1], damagePlusArray = new int[1], lifestealArray = new int[1], critArray = new int[1] }, 0);
    }

    public static int[] DetermineIntArrayValues(int[] toMod, int valMod, UpgradeLibrary.PlayerUpgrade up)
    {
        int[] returnArray = toMod;
        if ((up.affectsSword && up.affectsSpells && up.affectsGun) || (!up.affectsSword && !up.affectsSpells && !up.affectsGun))
        {
            for (int a = 0; a < returnArray.Length; a++)
            {
                returnArray[a] += valMod;
            }
        }
        else if (up.affectsSword) { returnArray[0] += valMod; }
        else if (up.affectsGun) { returnArray[1] += valMod; }
        else if (up.affectsSpells) { returnArray[2] += valMod; }
        return returnArray;
    }

    public static float[] DetermineFloatArrayValues(float[] toMod, float valMod, UpgradeLibrary.PlayerUpgrade up)
    {
        float[] returnArray = toMod;
        if ((up.affectsSword && up.affectsSpells && up.affectsGun) || (!up.affectsSword && !up.affectsSpells && !up.affectsGun))
        {
            for (int a = 0; a < returnArray.Length; a++)
            {
                returnArray[a] += valMod;
            }
        }
        else if (up.affectsSword) { returnArray[0] += valMod; }
        else if (up.affectsGun) { returnArray[1] += valMod; }
        else if (up.affectsSpells) { returnArray[2] += valMod; }
        return returnArray;
    }
}
