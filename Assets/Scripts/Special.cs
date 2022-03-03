using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Special : MonoBehaviour
{
    int special = 0;
    const int MAXVALUE = 500;

    public int Value
    {
        get
        {
            return special;
        }
        set
        {
            special = value;
            if (special < 0) { special = 0; }
            if (special > MAXVALUE) { special = MAXVALUE; }
            UI_HealthBarBrain.NotifyBrain(this);
        }
    }
}
