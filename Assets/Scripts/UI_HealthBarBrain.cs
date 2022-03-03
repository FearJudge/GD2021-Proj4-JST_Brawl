using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_HealthBarBrain : MonoBehaviour
{
    public GameObject playerHUDCanvasPreFab;
    public GameObject enemyHUDCanvasPreFab;
    public RectTransform playerHudStart;
    public RectTransform enemyHudStart;
    public Vector2 offSetFromPreviousPlayer;
    public Vector2 offSetFromPreviousEnemy;
    public Transform playerFolder;
    public Transform enemyFolder;

    private readonly string[] SUFFIXLIST = { " B", " C", " D", " E", " F", " G", " X", " Y", " Z", " Å", " Ä", " Ö" };
    private bool[] takenSuffixes = new bool[12];

    public int maximumEnemyCount = 3;
    public static UI_HealthBarBrain instance;

    public class LinkedHealth : IComparable<LinkedHealth>
    {
        public LinkedHealth(Health h, bool player)
        {
            hp = h;
            if (player) {
                prefab = Instantiate(instance.playerHUDCanvasPreFab, instance.playerFolder);
                spcomp = prefab.GetComponent<UI_SpecialBarComponent>();
                sp = h.gameObject.GetComponent<Special>();
            }
            else { prefab = Instantiate(instance.enemyHUDCanvasPreFab, instance.enemyFolder); }
            comp = prefab.GetComponent<UI_HealthBarComponent>();
            CreateHP(this);
        }

        public GameObject prefab;
        public UI_HealthBarComponent comp;
        public UI_SpecialBarComponent spcomp;
        public Health hp;
        public Special sp;

        public int CompareTo(LinkedHealth other)
        {
            if (other == null) { return 1; }
            return hp.lastTouched.CompareTo(other.hp.lastTouched);
        }
    }
    public List<LinkedHealth> playerHpInstances { get; set; } = new List<LinkedHealth>();
    public List<LinkedHealth> enemyHpInstances { get; set; } = new List<LinkedHealth>();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public static void UpdateHP(LinkedHealth h)
    {
        h.comp.UpdateHealthInformation(h.hp);
        SortAndHideEnemies(instance.maximumEnemyCount);
    }

    public static void UpdatePlayerHP(LinkedHealth h)
    {
        h.comp.UpdateHealthInformation(h.hp);
    }

    public static void UpdatePlayerSP(LinkedHealth h)
    {
        h.spcomp.UpdateSpecialInformation(h.sp);
    }

    public static void CreateHP(LinkedHealth h)
    {

        List<LinkedHealth> findHes = instance.enemyHpInstances.FindAll(p => p.hp.healthOwnerName == h.hp.healthOwnerName);
        if (findHes.Count > 0) { TakeSuffix(h, findHes.Count); }
        else { h.comp.SetUpInformation(h.hp.healthOwnerName, h.hp); }
    }

    private static void TakeSuffix(LinkedHealth h, int count)
    {
        string suffix = "";
        int suf = Mathf.Clamp(count - 1, 0, instance.SUFFIXLIST.Length - 1);
        for (int a = suf; a >= 0; a--)
        {
            suf = a;
            if (!instance.takenSuffixes[a]) { break; }
        }
        instance.takenSuffixes[suf] = true;
        suffix = instance.SUFFIXLIST[suf];
        h.comp.SetUpInformation(h.hp.healthOwnerName + suffix, h.hp, suf);
    }

    public static void SortAndHideEnemies(int cap)
    {
        instance.enemyHpInstances.Sort();
        int num = 0;
        for (int a = instance.enemyHpInstances.Count - 1; a >= 0; a--)
        {
            RectTransform currentEle = instance.enemyHpInstances[a].prefab.GetComponent<RectTransform>();
            if (num < cap)
            {
                currentEle.anchoredPosition = instance.enemyHudStart.anchoredPosition + (instance.offSetFromPreviousEnemy * num);
                currentEle.gameObject.SetActive(true);
            }
            else
            {
                currentEle.gameObject.SetActive(false);
            }
            num++;
        }
    }

    public static void SetPlayerBars()
    {
        for (int a = 0; a < instance.playerHpInstances.Count; a++)
        {
            RectTransform currentEle = instance.playerHpInstances[a].prefab.GetComponent<RectTransform>();
            currentEle.anchoredPosition = instance.playerHudStart.anchoredPosition + (instance.offSetFromPreviousPlayer * a);
        }
    }

    public static void NotifyBrain(Health HpInstance, string name, bool isPlayer)
    {
        if (!isPlayer) { EnemyFindHealth(HpInstance, name); }
        else { PlayerFindHealth(HpInstance, name); }
    }

    public static void NotifyBrain(Special SpInstance)
    {
        PlayerFindSpecial(SpInstance);
    }

    public static void EnemyFindHealth(Health HpInstance, string name)
    {
        LinkedHealth findH = instance.enemyHpInstances.Find(p => p.hp == HpInstance);
        if (findH != null) { UpdateHP(findH); }
        else { LinkedHealth newH = new LinkedHealth(HpInstance, false); instance.enemyHpInstances.Add(newH); SortAndHideEnemies(instance.maximumEnemyCount); }
    }

    public static void PlayerFindHealth(Health HpInstance, string name)
    {
        LinkedHealth findH = instance.playerHpInstances.Find(p => p.hp == HpInstance);
        if (findH != null) { UpdatePlayerHP(findH); }
        else { LinkedHealth newH = new LinkedHealth(HpInstance, true); instance.playerHpInstances.Add(newH); SetPlayerBars(); }
    }

    public static void PlayerFindSpecial(Special SpInstance)
    {
        LinkedHealth findS = instance.playerHpInstances.Find(p => p.sp == SpInstance);
        if (findS != null) { UpdatePlayerSP(findS); }
    }

    public static void NotifyBrainOfDeath(Health HpInstance, bool isPlayer)
    {
        if (!isPlayer) { NonPlayerKill(HpInstance); }
        else { NonPlayerKill(HpInstance); }
    }

    public static void NonPlayerKill(Health HpInstance)
    {
        LinkedHealth findH = instance.enemyHpInstances.Find(p => p.hp == HpInstance);
        if (findH == null) { return; }
        if (findH.comp.heldSuffixInd >= 0) { instance.takenSuffixes[findH.comp.heldSuffixInd] = false; }
        instance.enemyHpInstances.Remove(findH);
        Destroy(findH.prefab);
        SortAndHideEnemies(instance.maximumEnemyCount);
    }

    public static void PlayerIconChange(Health HpInstance, int currentWeapon)
    {
        LinkedHealth findH = instance.playerHpInstances.Find(p => p.hp == HpInstance);
        if (findH != null) { findH.comp.ChangeSprite(currentWeapon); }
    }

    public static void ClearMemoryOfEnemies()
    {
        for (int a = 0; a < instance.enemyHpInstances.Count; a++)
        {
            NonPlayerKill(instance.enemyHpInstances[a].hp);
        }
    }

    public static void PlayerKill(Health HpInstance)
    {
        // TODO: Add Life List System And Game Overs.
    }
}
