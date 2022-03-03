using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EncounterTrigger : MonoBehaviour
{
    public delegate void ActivateTrigger(EncounterTrigger instance);
    public static event ActivateTrigger PlayerReached;

    [HideInInspector] public BoxCollider trigger;
    public bool notAnEncounter = false;
    public UI_OnScreenEffectBrain.HintType hint;

    private void Awake()
    {
        trigger = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!notAnEncounter && other.gameObject.tag == "Player")
        {
            PlayerReached?.Invoke(this);
        }
        else if (hint != UI_OnScreenEffectBrain.HintType.None && other.gameObject.tag == "Player")
        {
            UI_OnScreenEffectBrain.DisplayHint(hint);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hint != UI_OnScreenEffectBrain.HintType.None && other.gameObject.tag == "Player")
        {
            UI_OnScreenEffectBrain.UnDisplayHint();
        }
    }
}
