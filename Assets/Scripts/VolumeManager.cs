using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public AudioMixer mixChannelSound;
    public Slider sfxSlider;
    public Slider volSlider;
    public TMPro.TextMeshProUGUI sfxPerc;
    public TMPro.TextMeshProUGUI volPerc;

    public void OnEnable()
    {
        SetSliders();
    }

    public void SetSFXSound(float value)
    {
        mixChannelSound.SetFloat("SFX", value);
        sfxPerc.text = ((80f + value) * 1.25f).ToString("000") + "%";
    }

    public void SetBGMSound(float value)
    {
        mixChannelSound.SetFloat("BGM", value);
        volPerc.text = ((80f + value) * 1.25f).ToString("000") + "%";
    }

    public void PlaySample()
    {
        SoundPlayer.PlaySFX(0, 1f, 1f);
    }

    public void SetSliders()
    {
        mixChannelSound.GetFloat("SFX", out float val);
        sfxSlider.value = val;
        mixChannelSound.GetFloat("BGM", out float valbg);
        volSlider.value = valbg;
    }
}
