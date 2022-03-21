using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SpecialBarComponent : MonoBehaviour
{
    public Slider specialBar;
    Color foundCol = Color.black;
    readonly int[] VALUETHRESHOLDS = { 60, 100, 200, 250, 475 };
    readonly Color[] COLORVALUES = {
        new Color(225f / 255f, 225f / 255f, 225f / 255f),
        new Color(225f / 255f, 225f / 255f, 225f / 255f),
        new Color(235f / 255f, 160f / 255f, 160f / 255f),
        new Color(235f / 255f, 140f / 255f, 140f / 255f),
        new Color(245f / 255f, 25f / 255f, 40f / 255f),
        new Color(255f / 255f, 0f / 255f, 0f / 255f)
    };
    [SerializeField] Animator barTrimAnim;

    public void UpdateSpecialInformation(Special s)
    {
        specialBar.value = s.Value;
        CheckColor(s.Value);
    }

    public void CheckColor(int special)
    {
        Color tempCol = Color.black;
        switch (special)
        {
            case int n when (n <= VALUETHRESHOLDS[0]):
                tempCol = COLORVALUES[0];
                barTrimAnim.SetInteger("SpecialFlash", 0);
                break;
            case int n when (n > VALUETHRESHOLDS[0] && n <= VALUETHRESHOLDS[1]):
                tempCol = COLORVALUES[1];
                barTrimAnim.SetInteger("SpecialFlash", 0);
                break;
            case int n when (n > VALUETHRESHOLDS[1] && n <= VALUETHRESHOLDS[2]):
                tempCol = COLORVALUES[2];
                barTrimAnim.SetInteger("SpecialFlash", 1);
                break;
            case int n when (n > VALUETHRESHOLDS[2] && n <= VALUETHRESHOLDS[3]):
                tempCol = COLORVALUES[3];
                barTrimAnim.SetInteger("SpecialFlash", 1);
                break;
            case int n when (n > VALUETHRESHOLDS[3] && n <= VALUETHRESHOLDS[4]):
                tempCol = COLORVALUES[4];
                barTrimAnim.SetInteger("SpecialFlash", 2);
                break;
            case int n when (n > VALUETHRESHOLDS[4]):
                tempCol = COLORVALUES[5];
                barTrimAnim.SetInteger("SpecialFlash", 2);
                break;
            default:
                break;
        }
        if (tempCol == foundCol || tempCol == Color.black) { return; }
        foundCol = tempCol;
        ColorBlock colblock = specialBar.colors;
        colblock.disabledColor = foundCol;
        specialBar.colors = colblock;
    }
}
