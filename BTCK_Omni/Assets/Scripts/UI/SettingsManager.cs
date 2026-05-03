using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : Panel
{
    [Header("Settings UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void OnEnable()
    {
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }
    }

    public void SaveAndClose()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.Save();
        Close();
    }

    public void BackWithoutSaving()
    {
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            OnMusicVolumeChanged(musicSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            OnSFXVolumeChanged(sfxSlider.value);
        }
        Close();
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetBackgroundMusicVolume(value);
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSoundEffectsVolume(value);
        }
    }
}