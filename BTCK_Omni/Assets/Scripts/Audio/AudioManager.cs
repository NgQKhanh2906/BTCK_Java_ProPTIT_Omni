using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public AudioMixer mainMixer;
    public AudioSource backgroundMusicSource;
    private AudioClip defaultBgm;
    public float soundEffectsVolume { get; private set; } = 1f;
    public float voiceVolume { get; private set; } = 1f;

    void Start()
    {
        if (backgroundMusicSource != null)
        {
            defaultBgm = backgroundMusicSource.clip;
        }
        LoadAudioSettings();
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        float db = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        mainMixer.SetFloat("MusicVol", db);

        PlayerPrefs.SetFloat("BackgroundMusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectsVolume = volume;
        float db = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        mainMixer.SetFloat("SFXVol", db);

        PlayerPrefs.SetFloat("SoundEffectsVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetVoiceVolume(float volume)
    {
        voiceVolume = volume;
        float db = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        mainMixer.SetFloat("VoiceVol", db);

        PlayerPrefs.SetFloat("VoiceVolume", volume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        SetBackgroundMusicVolume(PlayerPrefs.GetFloat("BackgroundMusicVolume", 1f));
        SetSoundEffectsVolume(PlayerPrefs.GetFloat("SoundEffectsVolume", 1f));
        SetVoiceVolume(PlayerPrefs.GetFloat("VoiceVolume", 1f));
    }

    public void PlayBGM(AudioClip clip)
    {
        if (backgroundMusicSource != null && clip != null)
        {
            backgroundMusicSource.clip = clip;
            backgroundMusicSource.Play();
        }
    }

    public void PlayDefaultBGM()
    {
        if (backgroundMusicSource != null && defaultBgm != null)
        {
            backgroundMusicSource.clip = defaultBgm;
            backgroundMusicSource.Play();
        }
    }
}