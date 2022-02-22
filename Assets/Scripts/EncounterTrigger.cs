using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EncounterTrigger : MonoBehaviour
{
    public delegate void ActivateTrigger(EncounterTrigger instance);
    public static event ActivateTrigger PlayerReached;

    [HideInInspector] public BoxCollider trigger;

    private void Awake()
    {
        trigger = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerReached?.Invoke(this);
        }
    }
}
