using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeLink : MonoBehaviour
{
    public static List<UpgradeLink> availableUpgrades = new List<UpgradeLink>();
    public static int availableInd = 0;

    [HideInInspector] public DepthBeUController ownedBy;
    [HideInInspector] public UpgradeLibrary.IUpgrade upgrade;
    [HideInInspector] public DepthBeUController player;

    [SerializeField] protected Image iconSlot;
    [SerializeField] protected Image iconBorder;
    [SerializeField] protected ParticleSystem ps;

    public void SelectedCurrent()
    {
        player.AddUpgradeToCharacter(upgrade);
        for (int a = 0; a < availableUpgrades.Count; a++)
        {
            Destroy(availableUpgrades[a].gameObject);
        }
        availableUpgrades.Clear();
        UpgradeLibrary.Clear();
        EncounterManager.Clear();
    }

    public void HoveringCurrent()
    {
        for (int a = 0; a < availableUpgrades.Count; a++) { availableUpgrades[a].ownedBy.sr.Color = Color.gray; }
        ownedBy.sr.Color = Color.magenta;
        UpgradeLibrary.DisplayUpgrade(availableUpgrades[availableInd].upgrade);
    }

    public void SetUp()
    {
        iconSlot.sprite = upgrade.icon;
    }

    public static void Scroll(int amount)
    {
        availableInd += amount;
        if (availableInd < 0) { availableInd = availableUpgrades.Count - 1; }
        else if (availableInd >= availableUpgrades.Count) { availableInd = 0; }
        UpgradeLibrary.instance.rotationOfUpgrades = availableInd * (360f / availableUpgrades.Count);
        UpgradeLibrary.instance.range = 0f;
        availableUpgrades[availableInd].HoveringCurrent();
    }

    public static void SelectCurrent()
    {
        availableUpgrades[availableInd].SelectedCurrent();
    }

    public static void ActivateInstances()
    {
        availableInd = 0;
        UpgradeLibrary.instance.linkRoot.localRotation = Quaternion.AngleAxis(0, Vector3.forward);
        float rotPerUpgrade = 360f / availableUpgrades.Count;
        for (int a = 0; a < availableUpgrades.Count; a++)
        {
            availableUpgrades[a].gameObject.SetActive(true);
            availableUpgrades[a].transform.Translate(Vector3.up * 250f, Space.World);
            availableUpgrades[a].transform.RotateAround(UpgradeLibrary.instance.linkRoot.position, Vector3.forward, a * -rotPerUpgrade);
        }
        Scroll(0);
    }

    public static void CreateInstance(DepthBeUController owner, DepthBeUController player, uint upgradeint)
    {
        GameObject nLink = Instantiate(UpgradeLibrary.instance.linkInstance, UpgradeLibrary.instance.linkRoot);
        UpgradeLink trueLink = nLink.GetComponent<UpgradeLink>();
        trueLink.ownedBy = owner;
        trueLink.player = player;
        trueLink.upgrade = UpgradeLibrary.GetUpgrade(upgradeint);
        trueLink.SetUp();
        nLink.SetActive(false);
        availableUpgrades.Add(trueLink);
    }
}
