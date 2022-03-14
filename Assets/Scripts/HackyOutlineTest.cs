using System.Collections;
using System.Collections.Generic;
using ToonBoom.Harmony;
using UnityEngine;

public class HackyOutlineTest : MonoBehaviour
{
    public HarmonyRenderer copy;
    private HarmonyRenderer myrend;
    public bool active = true;
    public float wavespeed = 5f;

    public Vector3 dir = Vector3.zero;
    public Color activateColor = Color.red;
    public float currentDist = 0f;
    float nextDist = 0f;
    public Vector2 vibrateRange = new Vector2(0.1f, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
        myrend = GetComponent<HarmonyRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            myrend.CurrentClipIndex = copy.CurrentClipIndex;
            myrend.CurrentFrame = copy.CurrentFrame;
            currentDist = Mathf.Lerp(currentDist, nextDist, Time.deltaTime * wavespeed);
            if (Mathf.Abs(currentDist - nextDist) <= 0.002f) { Activate(); }
            transform.localPosition = dir * currentDist;
        }
    }

    public void Activate()
    {
        active = true;
        nextDist = Random.Range(vibrateRange.x, vibrateRange.y);
        myrend.Color = activateColor;
    }

    public void DeActivate()
    {
        active = false;
        myrend.Color = Color.clear;
    }
}
