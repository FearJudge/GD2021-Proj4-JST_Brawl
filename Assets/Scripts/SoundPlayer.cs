using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public class SongWithData
    {
        public AudioClip song;
        public string artist = "-";
        public string songname = "-";
        public float starttime = 0f;
    }

    public static SoundPlayer instance;
    public List<AudioClip> sounds;
    public List<SongWithData> songs;
    [SerializeField] private AudioMixerGroup sfxMix;
    [SerializeField] private AudioMixerGroup bgmMix;
    private static List<GameObject> currentSounds = new List<GameObject>();
    private static GameObject currentBGM;
    private static GameObject crossfadeBGM;
    private float bgmvolume = 0.7f;
    private bool[] allowSound = new bool[5] { true, true, true, true, true };
    private bool bgmAudioFade = false;
    public bool AutoPlayBGM = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) { instance = this; }
        if (AutoPlayBGM) { PlayBGM(0, 0.8f, 1f); }
    }

    private void OnDestroy()
    {
        if (instance == this) { instance = null; }
    }

    void AdjustAudio()
    {
        if (bgmAudioFade) { return; }
        currentBGM.GetComponent<AudioSource>().volume = bgmvolume;
    }

    public void PlaySFX(int soundid)
    {
        PlaySFX(soundid, 0.7f, 1f);
    }

    public static void PlaySFX(int soundid, float volume, float pitch)
    {
        GameObject audio = new GameObject("SoundEffect", typeof(AudioSource));
        var source = audio.GetComponent<AudioSource>();
        source.outputAudioMixerGroup = instance.sfxMix;
        currentSounds.Add(audio);
        source.pitch = pitch;
        source.volume = volume;
        source.PlayOneShot(instance.sounds[soundid]);
        instance.StartCoroutine(PlayMe(source, audio));
    }

    public static void PlaySFX(AudioClip clip, float volume, float pitch)
    {
        GameObject audio = new GameObject("SoundEffect", typeof(AudioSource));
        var source = audio.GetComponent<AudioSource>();
        source.outputAudioMixerGroup = instance.sfxMix;
        currentSounds.Add(audio);
        source.pitch = pitch;
        source.volume = volume;
        source.PlayOneShot(clip);
        instance.StartCoroutine(PlayMe(source, audio));
    }

    public static void PlaySFXDelayed(float delay, AudioClip clip, float volume, float pitch)
    {
        GameObject audio = new GameObject("SoundEffect", typeof(AudioSource));
        var source = audio.GetComponent<AudioSource>();
        source.outputAudioMixerGroup = instance.sfxMix;
        currentSounds.Add(audio);
        source.pitch = pitch;
        source.volume = volume;
        instance.StartCoroutine(Delayed(clip, source,audio, delay));
    }

    public static void PlaySFXControlled(int id, float vol, float pitch, int channel = 0)
    {
        if (channel >= 5) { return; }
        if (!instance.allowSound[channel]) { return; }
        instance.allowSound[channel] = false;
        instance.Invoke(string.Format("AllowSoundAgain{0}", channel), 0.05f);
        PlaySFX(id, vol, pitch);
    }

    public static void PlaySFXControlled(AudioClip clip, float vol, float pitch, int channel = 0)
    {
        if (channel >= 5) { return; }
        if (!instance.allowSound[channel]) { return; }
        instance.allowSound[channel] = false;
        instance.Invoke(string.Format("AllowSoundAgain{0}", channel), 0.05f * (channel+1));
        PlaySFX(clip, vol, pitch);
    }

    void AllowSoundAgain0()
    {
        allowSound[0] = true;
    }
    void AllowSoundAgain1()
    {
        allowSound[1] = true;
    }
    void AllowSoundAgain2()
    {
        allowSound[2] = true;
    }
    void AllowSoundAgain3()
    {
        allowSound[3] = true;
    }
    void AllowSoundAgain4()
    {
        allowSound[4] = true;
    }

    public static void PlayBGM(float volume, float pitch)
    {
        PlayBGM(Random.Range(0, instance.songs.Count), volume, pitch);
    }

    public static void PlayBGM(int soundid, float volume, float pitch)
    {
        AudioSource player = BGMControl();
        instance.ControlBGM(player, 1f, 1f, instance.songs[soundid], volume);
    }

    public static void PlayBGM(AudioClip bgm, float volume, float pitch)
    {
        AudioSource player = BGMControl();
        instance.ControlBGM(player, 1f, 1f, new SongWithData() { song = bgm }, volume);
    }

    public static void CrossFadeBGM(int soundid, float volume, float pitch)
    {
        AudioSource player = BGMCrossFadeControl();
        AudioSource fader = crossfadeBGM.GetComponent<AudioSource>();
        instance.ControlBGM(fader);
        instance.ControlBGM(player, 1f, 1f, instance.songs[soundid], volume);
    }

    public static void StopBGM()
    {
        AudioSource player = BGMControl();
        instance.ControlBGM(player);
    }

    private void ControlBGM(AudioSource player, float outVol, float inVol, SongWithData song, float volume)
    {
        bgmAudioFade = true;
        player.outputAudioMixerGroup = bgmMix;
        StartCoroutine(FadeSwap(player, outVol, inVol, song, volume));
    }

    private void ControlBGM(AudioSource player)
    {
        bgmAudioFade = true;
        player.outputAudioMixerGroup = bgmMix;
        StartCoroutine(FadeSwap(player, 1.5f, 0f));
    }

    private static AudioSource BGMControl()
    {
        if (currentBGM == null)
        {
            GameObject audio = new GameObject("SoundTrack", typeof(AudioSource));
            currentBGM = audio;
        }
        var source = currentBGM.GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        return source;
    }

    private static AudioSource BGMCrossFadeControl()
    {
        crossfadeBGM = currentBGM;
        currentBGM = null;
        return BGMControl();
    }

    static IEnumerator Delayed(AudioClip toPlay, AudioSource source, GameObject sourceObject, float waitUntil)
    {
        yield return new WaitForSeconds(waitUntil);
        source.PlayOneShot(toPlay);
        instance.StartCoroutine(PlayMe(source, sourceObject));
    }

    static IEnumerator PlayMe(AudioSource src, GameObject obj)
    {
        while (src.isPlaying)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }
        currentSounds.Remove(obj);
        Destroy(obj);
        yield return null;
    }

    IEnumerator FadeSwap(AudioSource src, float timeOut, float timeIn, SongWithData newClip = null, float newVolumeIn = 0f)
    {
        float volumeStep = src.volume / 20f;
        if (src.isPlaying && timeOut > 0f)
        {
            for (int a = 0; a < 20; a++)
            {
                src.volume -= volumeStep;
                yield return new WaitForSeconds(timeOut / 20f);
            }
            src.Stop();
        }
        float volumeStepIn = newVolumeIn / 20f;
        if (volumeStepIn < 0 || volumeStepIn > 0.05f) { volumeStepIn = volumeStep; if (volumeStepIn < 0 || volumeStepIn > 0.05f) { volumeStep = 0.05f; } }
        if (newClip == null || timeIn == 0f) { yield break; }
        src.clip = newClip.song;
        if (newClip.starttime != 0f) { src.time = newClip.starttime; }
        src.Play();
        src.volume = 0f;
        for (int a = 0; a < 20; a++)
        {
            src.volume += volumeStepIn;
            yield return new WaitForSeconds(timeIn / 20f);
        }
        if (bgmAudioFade == true) { bgmAudioFade = false; AdjustAudio(); }
    }
}
