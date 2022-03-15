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
        public int[] bloodStealArray;
        public float[] resistanceModArray;
        public int extraProj;
    }

    [SerializeField] public readonly string moveName = "";
    public bool triggerIsBool = false;
    public bool setTo = false;
    public string animationTrigger = "";
    public AudioClip sound = null;
    public float[] playDelays = new float[0];
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
    public int useSpawner = 0;
    public int projCount = 1;
    public Vector3 projVelocity = Vector3.zero;
    public bool isProjBuff;
    public bool isStacking;
    public int selfDamage = 0;
    public bool nonAttack = false;
    public LayerMask alternateLayerMask;
    ProjectileSpawner projectilespawner;

    public void ActivateMove(DepthBeUController player, GlobalVariables pv, int type, float multiplier = 1f)
    {
        void ProjectileBuff()
        {
            if (!isStacking) { projectilespawner.SetStateToRegular(); }
            projectilespawner.lifeSteal += lifeSteal;
            projectilespawner.ammo += ammoChange;
            projectilespawner.extraVelocity += projVelocity;
            projectilespawner.extraDamage += hurtBoxDamage;
            projectilespawner.extraCrit += baseCrit;
            projectilespawner.extraStun += hitStunDuration;
            projectilespawner.firedExtraProjectiles += projCount;
        }

        if (player == null) { Debug.Log("No Controller!"); return; }
        if (sound != null)
        {
            if (playDelays.Length == 0) { SoundPlayer.PlaySFXControlled(sound, 1f, 1f, 2); }
            else
            {
                foreach (float delay in playDelays)
                {
                    if (delay < 0f) { continue; }
                    SoundPlayer.PlaySFXDelayed(delay, sound, 1f, 1f);
                }
            }
        }
        if (triggerIsBool) { player.animator.SetBool(animationTrigger, setTo); }
        else if (animationTrigger != "") { player.animator.SetTrigger(animationTrigger); }
        if (animationSpeedMod != 0f) { player.animator.speed = animationSpeedMod; }
        player.SetVelocity(characterVelocity, true);
        if (characterVelocity.y > 1f) { player.airborne = true; }
        player.hb.damage = Mathf.CeilToInt((hurtBoxDamage + pv.damagePlusArray[type]) * (1f + pv.damageModArray[type]) * multiplier);
        player.hb.hitStun = hitStunDuration * (1f + pv.hitstunArray[type]);
        player.hb.knockDown = knockDown;
        player.hb.knockDownVelocity = knockDownVelocity;
        player.hb.lifeSteal = lifeSteal + pv.lifestealArray[type];
        player.hb.bloodSteal = pv.bloodStealArray[type];
        player.hb.resistanceMod = pv.resistanceModArray[type];
        player.hb.crit = pv.critArray[type] + baseCrit;
        if (selfDamage != 0) { player.hpScript.Hp -= selfDamage; }
        if (player.projectilespawners.Length > 0 && useSpawner < player.projectilespawners.Length)
        { projectilespawner = player.projectilespawners[useSpawner]; }
        if (isProjectile && projectilespawner != null) { projectilespawner.firedProjectiles = projCount + pv.extraProj; projectilespawner.hbData = player.hb; projectilespawner.ammoChange = ammoChange; projectilespawner.vel = projVelocity; }
        if (isProjBuff)
        {      
            if (projectilespawner != null) { ProjectileBuff(); }
            else { for (int i = 0; i < player.projectilespawners.Length; i++) { projectilespawner = player.projectilespawners[i]; ProjectileBuff(); } }
        }
        if (nonAttack) { player.hb.useAlternate = true; player.hb.activeOnAlt = alternateLayerMask; }
        else { player.hb.useAlternate = false; }
    }

    public void ActivateMove(DepthBeUController player)
    {
        ActivateMove(player, new GlobalVariables() { damageModArray = new float[1], hitstunArray = new float[1], damagePlusArray = new int[1], lifestealArray = new int[1], critArray = new int[1], extraProj = 0, bloodStealArray = new int[1], resistanceModArray = new float[1] }, 0);
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
