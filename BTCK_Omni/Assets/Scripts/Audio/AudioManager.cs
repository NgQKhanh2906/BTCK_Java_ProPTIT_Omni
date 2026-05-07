using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public AudioSource backgroundMusicSource;
    public float soundEffectsVolume { get; private set; } = 0.6f;

    void Start()
    {
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
}