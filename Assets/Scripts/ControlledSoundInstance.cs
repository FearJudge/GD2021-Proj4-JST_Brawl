using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlledSoundInstance : MonoBehaviour
{
    [SerializeField] AudioClip[] toPlay = new AudioClip[0];
    public float volMod = 1f;
    public float pitch = 1f;

    // Start is called before the first frame update
    void Start()
    {
        SoundPlayer.PlaySFX(toPlay[Random.Range(0, toPlay.Length - 1)], volMod, pitch);
    }
}
