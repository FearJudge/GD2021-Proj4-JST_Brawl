using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_HealthBarComponent : MonoBehaviour
{
    public TMPro.TextMeshProUGUI characterNameDisplay;
    public UnityEngine.UI.Slider healthBar;
    public UnityEngine.UI.Slider healthBarFollower;
    public UnityEngine.UI.Image characterIcon;
    public Sprite[] sprites = new Sprite[] { };
    int curInd = 0;
    public int heldSuffixInd = -1;
    bool movin = false;
    int movetow = 0;

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
        else { healthBar.value = hp.Hp; }
        movetow = hp.Hp;
        StartCoroutine(HPEffect());
    }

    public void ChangeSprite(int defValue, bool wrap = false)
    {
        if (sprites.Length > defValue && defValue >= 0) { characterIcon.sprite = sprites[defValue]; curInd = defValue; }
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
            else { healthBar.value += 1; yield return new WaitForSeconds(0.2f); }
            yield return new WaitForSeconds(0.01f);
        }
        movin = false;
    }
}
