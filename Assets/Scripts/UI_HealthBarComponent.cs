using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_HealthBarComponent : MonoBehaviour
{
    public TMPro.TextMeshProUGUI characterNameDisplay;
    public UnityEngine.UI.Slider healthBar;
    public UnityEngine.UI.Image characterIcon;
    public Sprite[] sprites = new Sprite[] { };
    int curInd = 0;
    public int heldSuffixInd = -1;

    public void SetUpInformation(string name, Health hp, int suffix = -1)
    {
        heldSuffixInd = suffix;
        characterNameDisplay.text = name;
        UpdateHealthInformation(hp);
        if (sprites.Length > 0) { characterIcon.sprite = sprites[0]; }
    }

    public void UpdateHealthInformation(Health hp)
    {
        healthBar.maxValue = hp.GetMaxHealth();
        healthBar.value = hp.Hp;
    }

    public void ChangeSprite(int defValue, bool wrap = false)
    {
        if (sprites.Length > defValue && defValue >= 0) { characterIcon.sprite = sprites[defValue]; curInd = defValue; }
    }

    public void AdjustSprite(int adjValue)
    {
        ChangeSprite(curInd + adjValue);
    }
}
