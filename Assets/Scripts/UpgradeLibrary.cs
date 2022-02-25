using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeLibrary : MonoBehaviour
{
    public static UpgradeLibrary instance;
    [System.Serializable]
    public abstract class IUpgrade
    {
        public string upgradeName = "";
        [TextArea] public string upgradeDescription = "";
        public uint upgradeId = 0;
        public Sprite icon;
    }

    [System.Serializable]
    public class MoveUpgrade : IUpgrade
    {
        public string onMove = "";
        public bool isNewMove = false;

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

    [System.Serializable]
    public class PlayerUpgrade : IUpgrade
    {
        public string DetermineTarget()
        {
            int targettype = 0;
            targettype += affectsSword ? 1 : 0;
            targettype += affectsGun ? 2 : 0;
            targettype += affectsSpells ? 4 : 0;
            switch (targettype)
            {
                case 1:
                    return "Sword Upgrade";
                case 2:
                    return "Gun Upgrade";
                case 4:
                    return "Spell Upgrade";
                default:
                    return "Global Upgrade";
            }
        }
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

    public const int BREAKPOINT = 1000;
    public PlayerUpgrade[] playerUpgrades = new PlayerUpgrade[0];
    public MoveUpgrade[] moveUpgrades = new MoveUpgrade[0];
    public GameObject linkInstance;
    public Transform linkRoot;
    public RectTransform linkPosition;

    [SerializeField] protected TMPro.TextMeshProUGUI upgradeName;
    [SerializeField] protected TMPro.TextMeshProUGUI upgradeTarget;
    [SerializeField] protected TMPro.TextMeshProUGUI upgradeDescription;

    public float rotationOfUpgrades = 0f;
    public float range = 0;

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (linkRoot.rotation.eulerAngles.z != rotationOfUpgrades)
        {
            range += Time.deltaTime * 0.4f;
            range = Mathf.Clamp(range, 0, 1f);
            linkRoot.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(linkRoot.rotation.eulerAngles.z, rotationOfUpgrades, range));
        }
        else { range = 1; }
        if (range == 1) { range = 0f; }
    }

    public static void DisplayUpgrade(IUpgrade toDisplay)
    {
        instance.upgradeName.gameObject.SetActive(true);
        instance.upgradeDescription.gameObject.SetActive(true);
        instance.upgradeTarget.gameObject.SetActive(true);
        instance.upgradeName.text = toDisplay.upgradeName;
        instance.upgradeDescription.text = toDisplay.upgradeDescription;
        if (toDisplay.upgradeId < BREAKPOINT)
        {
            PlayerUpgrade displayedUpgrade = (PlayerUpgrade)toDisplay;
            instance.upgradeTarget.text = displayedUpgrade.DetermineTarget();
        }
        else
        {
            MoveUpgrade displayedUpgrade = (MoveUpgrade)toDisplay;
            instance.upgradeTarget.text = displayedUpgrade.onMove;
            if (displayedUpgrade.isNewMove) { instance.upgradeTarget.text += " [NEW!]"; }
            else { instance.upgradeTarget.text += " Upgrade"; }
        }
    }

    public static void Clear()
    {
        instance.upgradeName.text = "";
        instance.upgradeDescription.text = "";
        instance.upgradeTarget.text = "";
        instance.upgradeName.gameObject.SetActive(false);
        instance.upgradeDescription.gameObject.SetActive(false);
        instance.upgradeTarget.gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        instance = null;
    }

    public static IUpgrade GetUpgrade(uint id)
    {
        IUpgrade found = null;
        if (id < BREAKPOINT)
        {
            for (int a = 0; a < instance.playerUpgrades.Length; a++)
            {
                if (instance.playerUpgrades[a].upgradeId == id) { found = instance.playerUpgrades[a]; break; }
            }
        }
        else
        {
            for (int a = 0; a < instance.moveUpgrades.Length; a++)
            {
                if (instance.moveUpgrades[a].upgradeId == id) { found = instance.moveUpgrades[a]; break; }
            }
        }
        return found;
    }

    public static uint RandomUpgrade()
    {
        int rand = Random.Range(0, 2);
        uint id = 0;
        if (rand == 0)
        {
            id = instance.playerUpgrades[Random.Range(0, instance.playerUpgrades.Length - 1)].upgradeId;
        }
        else
        {
            id = instance.moveUpgrades[Random.Range(0, instance.moveUpgrades.Length - 1)].upgradeId;
        }
        return id;
    }
}
