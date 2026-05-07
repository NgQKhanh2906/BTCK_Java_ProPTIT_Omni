using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public AudioSource backgroundMusicSource;
    private AudioClip defaultBGM;
    public float soundEffectsVolume { get; private set; } = 0.6f;

    void Start()
    {
        if (backgroundMusicSource != null)
        {
            defaultBGM = backgroundMusicSource.clip;
        }
        LoadAudioSettings();
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = volume;
        }

        PlayerPrefs.SetFloat("BackgroundMusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectsVolume = volume;
        PlayerPrefs.SetFloat("SoundEffectsVolume", volume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        float backgroundMusicVolume = PlayerPrefs.GetFloat("BackgroundMusicVolume", 1f);
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = backgroundMusicVolume;
        }

        soundEffectsVolume = PlayerPrefs.GetFloat("SoundEffectsVolume", 1f);
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
        if (backgroundMusicSource != null && defaultBGM != null)
        {
            backgroundMusicSource.clip = defaultBGM;
            backgroundMusicSource.Play();
        }
    }
}