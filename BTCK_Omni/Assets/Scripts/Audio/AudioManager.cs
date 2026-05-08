using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public AudioSource backgroundMusicSource;
    public AudioMixer mainMixer;
    public float soundEffectsVolume { get; private set; } = 1f;
    private AudioClip oldBgm;

    [Header("Max Volume Setup (dB)")]
    public float musicMaxDecibel = 0f;
    public float sfxMaxDecibel = 1f;
    public float voiceMaxDecibel = 10f;

    void Start()
    {
        if (backgroundMusicSource != null)
        {
            oldBgm = backgroundMusicSource.clip;
        }
        LoadAudioSettings();
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        float musicDecibel;
        float voiceDecibel;

        if (volume <= 0.0001f)
        {
            musicDecibel = -80f;
            voiceDecibel = -80f;
        }
        else
        {
            musicDecibel = Mathf.Log10(volume) * 20f + musicMaxDecibel;
            voiceDecibel = Mathf.Log10(volume) * 20f + voiceMaxDecibel;
        }

        if (mainMixer != null)
        {
            mainMixer.SetFloat("MusicVolumeParam", musicDecibel);
            mainMixer.SetFloat("VoiceVolumeParam", voiceDecibel);
        }

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectsVolume = volume;
        float sfxDecibel;

        if (volume <= 0.0001f)
        {
            sfxDecibel = -80f;
        }
        else
        {
            sfxDecibel = Mathf.Log10(volume) * 20f + sfxMaxDecibel;
        }

        if (mainMixer != null)
        {
            mainMixer.SetFloat("SFXVolumeParam", sfxDecibel);
        }

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SetBackgroundMusicVolume(musicVolume);

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        SetSoundEffectsVolume(sfxVolume);
    }

    public void ChangeBGM(AudioClip clip)
    {
        if (backgroundMusicSource != null && clip != null)
        {
            backgroundMusicSource.clip = clip;
            backgroundMusicSource.Play();
        }
    }

    public void ResumeBGM()
    {
        if (backgroundMusicSource != null && oldBgm != null)
        {
            backgroundMusicSource.clip = oldBgm;
            backgroundMusicSource.Play();
        }
    }
}