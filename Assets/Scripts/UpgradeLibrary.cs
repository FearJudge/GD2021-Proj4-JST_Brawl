using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeLibrary : MonoBehaviour
{
    public static UpgradeLibrary instance;
    public abstract class IUpgrade
    {
        public string upgradeName = "";
        public uint upgradeId = 0;
    }

    public class MoveUpgrade : IUpgrade
    {
        public string onMove = "";

        public int addLifeSteal = 0;
        public int addCriticalChance = 0;
        public float knockBackModifier = 0;
        public float damageModifier = 0;
        public float stunModifier = 0;

        public float changeSpeedOfAnimation = 0f;
        public int changeAttackPrevention = 0;
        public int changeMovementPrevention = 0;
        public int changeFollowUpTiming = 0;
        public Vector3 playerVelChange = Vector3.zero;

        public string addFollowUps = "";
        public string removeFollowUps = "";


        // knockdown: 0 = no change, 1 = change to FALSE, 2 = change to TRUE, 3 = SWAP
        public int changeKnockDown = 0;
        public Vector3 knockdownChange = Vector3.zero;
    }

    public class PlayerUpgrade : IUpgrade
    {
        public int addHealth = 0;
        public int addLifeSteal = 0;
        public int addCriticalChance = 0;
        public float knockBackModifier = 0;
        public float animationSpeedModifier = 0;
        public float healthModifier = 0;
        public float damageModifier = 0;
        public float stunModifier = 0;
        public float movementModifier = 0;
        public bool affectsSword = true;
        public bool affectsGun = true;
        public bool affectsSpells = true;
    }

    public void Start()
    {
        instance = this;
    }

    public void OnDestroy()
    {
        instance = null;
    }
}
