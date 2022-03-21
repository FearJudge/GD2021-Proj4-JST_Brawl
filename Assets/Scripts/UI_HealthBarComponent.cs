using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_HealthBarComponent : MonoBehaviour
{
    public TMPro.TextMeshProUGUI characterNameDisplay;
    public TMPro.TextMeshProUGUI weaponNameDisplay;
    public UnityEngine.UI.Slider healthBar;
    public UnityEngine.UI.Slider healthBarFollower;
    public UnityEngine.UI.Image characterIcon;
    public Sprite[] sprites = new Sprite[] { };
    public string[] weapnNames = new string[] { };
    int curInd = 0;
    public int heldSuffixInd = -1;
    bool movin = false;
    int movetow = 0;
    [SerializeField] Animator barIconAnim;
    [SerializeField] Animator barHealthAnim;
    [SerializeField] Animator weapnNameAnim;

    public void SetUpInformation(string name, Health hp, int suffix = -1)
    {
        heldSuffixInd = suffix;
        characterNameDisplay.text = name;
        healthBar.wholeNumbers = true;
        healthBarFollower.wholeNumbers = true;
        UpdateHealthInformation(hp, true);
        if (sprites.Length > 0) { characterIcon.sprite = sprites[hp.iconid]; }
    }

    public void UpdateHealthInformation(Health hp, bool setValues=false)
    {
        if (!gameObject.activeSelf) { return; }
        healthBarFollower.maxValue = hp.GetMaxHealth();
        healthBar.maxValue = hp.GetMaxHealth();
        if (setValues) { healthBar.value = hp.Hp; healthBarFollower.value = healthBar.value; }
        if (hp.Hp > healthBar.value) { healthBarFollower.value = hp.Hp; }
        else { if (hp.Hp < healthBar.value) { TakeDamage(); } healthBar.value = hp.Hp;  }
        movetow = hp.Hp;
        SetDangerState(hp);
        StartCoroutine(HPEffect());
    }

    public void TakeDamage()
    {
        if (barIconAnim == null) { return; }
        barIconAnim.SetTrigger("FLASH");
    }

    public void SetDangerState(Health hp)
    {
        if (barHealthAnim == null) { return; }
        float perc = hp.Hp / (float)hp.GetMaxHealth();
        switch (perc)
        {
            case float n when n >= 0.65f:
                barHealthAnim.SetInteger("Danger", 0);
                break;
            case float n when n < 0.65f && n >= 0.35f:
                barHealthAnim.SetInteger("Danger", 1);
                break;
            case float n when n < 0.35f && n >= 0.15f:
                barHealthAnim.SetInteger("Danger", 2);
                break;
            case float n when n < 0.15f:
                barHealthAnim.SetInteger("Danger", 3);
                break;
            default:
                break;
        }
    }

    public void ChangeSprite(int defValue, bool wrap = false)
    {
        if (sprites.Length > defValue && defValue >= 0) { characterIcon.sprite = sprites[defValue]; curInd = defValue; }
        if (weaponNameDisplay == null) { return; }
        weaponNameDisplay.text = weapnNames[curInd];
        weapnNameAnim.SetTrigger("Display");
    }

    public void AdjustSprite(int adjValue)
    {
        ChangeSprite(curInd + adjValue);
    }

    public IEnumerator HPEffect()
    {
        if (movin || gameObject == null || !gameObject.activeSelf) { yield break; }
        movin = true;
        while (healthBar.value != healthBarFollower.value)
        {
            if (healthBarFollower.value > movetow) { healthBarFollower.value -= 1; }
            else { healthBar.value += 1; yield return new WaitForSeconds(0.01f); }
            yield return new WaitForSeconds(0.01f);
        }
        movin = false;
    }

    public void OnDisable()
    {
        movin = false;
    }
}
