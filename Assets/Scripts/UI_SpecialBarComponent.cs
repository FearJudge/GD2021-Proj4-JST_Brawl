using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SpecialBarComponent : MonoBehaviour
{
    public Slider specialBar;
    Color foundCol = Color.black;
    readonly int[] VALUETHRESHOLDS = { 200, 240, 280, 320, 425 };
    readonly Color[] COLORVALUES = {
        new Color(180f / 255f, 180f / 255f, 180f / 255f),
        new Color(200f / 255f, 170f / 255f, 170f / 255f),
        new Color(220f / 255f, 155f / 255f, 155f / 255f),
        new Color(240f / 255f, 140f / 255f, 140f / 255f),
        new Color(255f / 255f, 120f / 255f, 120f / 255f),
        new Color(255f / 255f, 30f / 255f, 30f / 255f)
    };

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
                break;
            case int n when (n > VALUETHRESHOLDS[0] && n <= VALUETHRESHOLDS[1]):
                tempCol = COLORVALUES[1];
                break;
            case int n when (n > VALUETHRESHOLDS[1] && n <= VALUETHRESHOLDS[2]):
                tempCol = COLORVALUES[2];
                break;
            case int n when (n > VALUETHRESHOLDS[2] && n <= VALUETHRESHOLDS[3]):
                tempCol = COLORVALUES[3];
                break;
            case int n when (n > VALUETHRESHOLDS[3] && n <= VALUETHRESHOLDS[4]):
                tempCol = COLORVALUES[4];
                break;
            case int n when (n > VALUETHRESHOLDS[4]):
                tempCol = COLORVALUES[5];
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
