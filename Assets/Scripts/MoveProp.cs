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
    }

    [SerializeField] public readonly string moveName = "";
    public string animationTrigger = "";
    public float animationSpeedMod = 1f;
    public int allowNextMove = 50;
    public int preventMovement = 10;
    public int followUpAllowFrom = 0;
    public Vector3 characterVelocity = Vector3.zero;
    public int hurtBoxDamage = 10;
    public int lifeSteal = 0;
    public float hitStunDuration = 0.25f;
    public bool knockDown = false;
    public Vector3 knockDownVelocity = Vector3.zero;

    public void ActivateMove(DepthBeUController player, GlobalVariables pv, int type)
    {
        if (player == null) { Debug.Log("No Controller!"); return; }
        if (animationTrigger != "") { player.animator.SetTrigger(animationTrigger); }
        player.SetVelocity(characterVelocity, true);
        if (characterVelocity.y > 1f) { player.airborne = true; }
        player.hb.damage = Mathf.CeilToInt((hurtBoxDamage + pv.damagePlusArray[type]) * (1f + pv.damageModArray[type]));
        player.hb.hitStun = hitStunDuration * (1f + pv.hitstunArray[type]);
        player.hb.knockDown = knockDown;
        player.hb.knockDownVelocity = knockDownVelocity;
        player.hb.lifeSteal = lifeSteal + pv.lifestealArray[type];
    }

    public void ActivateMove(DepthBeUController player)
    {
        ActivateMove(player, new GlobalVariables() { damageModArray = new float[1], hitstunArray = new float[1], damagePlusArray = new int[1], lifestealArray = new int[1] }, 0);
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
