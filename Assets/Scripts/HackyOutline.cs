using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackyOutline : MonoBehaviour
{
    public HackyOutlineTest[] dirs;

    public void ActivateOutlines()
    {
        foreach (HackyOutlineTest hacky in dirs)
        {
            hacky.Activate();
        }
    }

    public void DeactivateOutlines()
    {
        foreach (HackyOutlineTest hacky in dirs)
        {
            hacky.DeActivate();
        }
    }
}
