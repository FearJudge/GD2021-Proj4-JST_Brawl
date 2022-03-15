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
        UpgradeLibrary.DisplayUpgrade(availableUpgrades[availableInd].upgrade);
        if (ownedBy == null) { return; }
        for (int a = 0; a < availableUpgrades.Count; a++) { availableUpgrades[a].ownedBy.sr.Color = Color.gray; }
        ownedBy.sr.Color = Color.magenta;
    }

    private void OnDestroy()
    {
        if (availableUpgrades.Contains(this))
        {
            availableUpgrades.Remove(this);
        }
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

    public static bool ActivateInstances()
    {
        availableInd = 0;
        UpgradeLibrary.instance.linkRoot.localRotation = Quaternion.AngleAxis(0, Vector3.forward);
        if (availableUpgrades.Count == 0) { return false; }
        float rotPerUpgrade = 360f / availableUpgrades.Count;
        for (int a = 0; a < availableUpgrades.Count; a++)
        {
            availableUpgrades[a].gameObject.SetActive(true);
            RectTransform availableRect = availableUpgrades[a].GetComponent<RectTransform>();
            availableRect.anchoredPosition = UpgradeLibrary.instance.linkPosition.anchoredPosition;
            availableUpgrades[a].transform.RotateAround(UpgradeLibrary.instance.linkRoot.position, Vector3.forward, a * -rotPerUpgrade);
        }
        Scroll(0);
        return true;
    }

    public static void CreateInstance(DepthBeUController owner, DepthBeUController player, uint upgradeint)
    {
        if (upgradeint == 0 || UpgradeLibrary.GetUpgrade(upgradeint) == null) { return; }
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
